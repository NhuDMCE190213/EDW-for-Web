using System.Collections.Concurrent;
using PresentationMVC.Models;
using PresentationMVC.Models.Cart;
using PresentationMVC.Models.Order.Customer;
using PresentationMVC.Models.Order.Staff;

namespace PresentationMVC.Services
{
    public class CartService : ICartService
    {
        private readonly ProductApiClient _productApi;
        private readonly ProductVariantApiClient _variantApi;
        private readonly CustomerOrderApiClient _customerOrderApi;
        private readonly OrderApiClient _staffOrderApi;
        private readonly ILogger<CartService> _logger;

        // In-memory persistent cart store per customer
        private static readonly ConcurrentDictionary<int, List<CartItemDto>> _customerCarts = new();
        // In-memory persistent orders store per customer (supplements/fallbacks API orders)
        private static readonly ConcurrentDictionary<int, List<OrderDto>> _localCustomerOrders = new();
        private static int _nextCartItemId = 1000;

        public CartService(
            ProductApiClient productApi,
            ProductVariantApiClient variantApi,
            CustomerOrderApiClient customerOrderApi,
            OrderApiClient staffOrderApi,
            ILogger<CartService> logger)
        {
            _productApi = productApi;
            _variantApi = variantApi;
            _customerOrderApi = customerOrderApi;
            _staffOrderApi = staffOrderApi;
            _logger = logger;
        }

        public Task<List<CartItemDto>> GetCartItemsAsync(int customerId, CancellationToken cancellationToken = default)
        {
            var cart = _customerCarts.GetOrAdd(customerId, _ => new List<CartItemDto>());
            lock (cart)
            {
                return Task.FromResult(cart.Select(CloneCartItem).ToList());
            }
        }

        public Task<int> GetCartItemCountAsync(int customerId, CancellationToken cancellationToken = default)
        {
            var cart = _customerCarts.GetOrAdd(customerId, _ => new List<CartItemDto>());
            lock (cart)
            {
                return Task.FromResult(cart.Sum(i => i.Quantity));
            }
        }

        public async Task<CartItemDto> AddToCartAsync(int customerId, Guid variantId, int quantity, CancellationToken cancellationToken = default)
        {
            if (quantity <= 0) quantity = 1;

            var variantInfo = await FindVariantAsync(variantId, cancellationToken);
            if (variantInfo == null)
            {
                throw new ArgumentException($"Product variant not found.");
            }

            if (variantInfo.StockQuantity <= 0)
            {
                throw new InvalidOperationException($"This product variant is currently out of stock.");
            }

            var cart = _customerCarts.GetOrAdd(customerId, _ => new List<CartItemDto>());
            lock (cart)
            {
                var existing = cart.FirstOrDefault(c => c.ProductVariantId == variantId);
                if (existing != null)
                {
                    int newQty = existing.Quantity + quantity;
                    if (newQty > variantInfo.StockQuantity)
                    {
                        throw new InvalidOperationException($"Cannot add {quantity} item(s). Only {variantInfo.StockQuantity - existing.Quantity} available in stock.");
                    }
                    existing.Quantity = newQty;
                    existing.ProductVariant = variantInfo;
                    return CloneCartItem(existing);
                }
                else
                {
                    if (quantity > variantInfo.StockQuantity)
                    {
                        throw new InvalidOperationException($"Cannot add {quantity} item(s). Only {variantInfo.StockQuantity} available in stock.");
                    }

                    var newItem = new CartItemDto
                    {
                        CartItemId = Interlocked.Increment(ref _nextCartItemId),
                        CustomerId = customerId,
                        ProductVariantId = variantId,
                        Quantity = quantity,
                        ProductVariant = variantInfo
                    };
                    cart.Add(newItem);
                    return CloneCartItem(newItem);
                }
            }
        }

        public async Task<bool> UpdateQuantityAsync(int customerId, int cartItemId, int quantity, CancellationToken cancellationToken = default)
        {
            if (quantity <= 0)
            {
                return await RemoveItemAsync(customerId, cartItemId, cancellationToken);
            }

            var cart = _customerCarts.GetOrAdd(customerId, _ => new List<CartItemDto>());
            lock (cart)
            {
                var item = cart.FirstOrDefault(i => i.CartItemId == cartItemId);
                if (item == null) return false;

                if (item.ProductVariant != null && quantity > item.ProductVariant.StockQuantity)
                {
                    throw new InvalidOperationException($"Cannot update quantity to {quantity}. Only {item.ProductVariant.StockQuantity} items are available in stock.");
                }

                item.Quantity = quantity;
                return true;
            }
        }

        public Task<bool> RemoveItemAsync(int customerId, int cartItemId, CancellationToken cancellationToken = default)
        {
            var cart = _customerCarts.GetOrAdd(customerId, _ => new List<CartItemDto>());
            lock (cart)
            {
                var item = cart.FirstOrDefault(i => i.CartItemId == cartItemId);
                if (item == null) return Task.FromResult(false);
                cart.Remove(item);
                return Task.FromResult(true);
            }
        }

        public async Task<OrderCheckoutResultDto> CheckoutAsync(int customerId, List<int> selectedCartItemIds, CancellationToken cancellationToken = default)
        {
            if (customerId <= 0)
            {
                throw new ArgumentException("Customer must be logged in to checkout.");
            }

            if (selectedCartItemIds == null || selectedCartItemIds.Count == 0)
            {
                throw new ArgumentException("Please select at least one item to proceed.");
            }

            var cart = _customerCarts.GetOrAdd(customerId, _ => new List<CartItemDto>());
            List<CartItemDto> itemsToCheckout;

            lock (cart)
            {
                itemsToCheckout = cart.Where(i => selectedCartItemIds.Contains(i.CartItemId)).ToList();
                if (itemsToCheckout.Count == 0)
                {
                    throw new InvalidOperationException("Selected items could not be found in your cart.");
                }

                // Check stock
                foreach (var item in itemsToCheckout)
                {
                    if (item.ProductVariant != null && item.Quantity > item.ProductVariant.StockQuantity)
                    {
                        throw new InvalidOperationException($"Insufficient stock for {item.ProductVariant.ProductName}. Only {item.ProductVariant.StockQuantity} available.");
                    }
                }
            }

            decimal subtotal = 0m;
            decimal discount = 0m;
            decimal shipping = 0m;
            var orderId = Guid.NewGuid();
            var orderItems = new List<OrderItemDto>();

            foreach (var item in itemsToCheckout)
            {
                var v = item.ProductVariant;
                decimal unitPrice = v?.Price ?? 0m;
                decimal originalPrice = v?.OriginalPrice ?? unitPrice;
                if (originalPrice > unitPrice)
                {
                    discount += (originalPrice - unitPrice) * item.Quantity;
                }

                subtotal += (v?.OriginalPrice ?? unitPrice) * item.Quantity;

                var orderItem = new OrderItemDto
                {
                    OrderItemId = new Random().Next(1000, 9999),
                    OrderId = orderId,
                    ProductVariantId = item.ProductVariantId,
                    ProductName = v?.ProductName ?? "Product",
                    ProductVariantDetails = $"{v?.Color} {v?.GetFormattedSpecs()}".Trim(),
                    Quantity = item.Quantity,
                    PriceAtPurchase = unitPrice,
                    CreatedAt = DateTime.UtcNow
                };
                orderItems.Add(orderItem);

                // Deduct stock locally
                if (v != null)
                {
                    v.StockQuantity = Math.Max(0, v.StockQuantity - item.Quantity);
                }
            }

            decimal total = subtotal - discount + shipping;
            if (total < 0) total = 0;

            var newOrder = new OrderDto
            {
                OrderId = orderId,
                CustomerId = customerId,
                CustomerName = "Customer",
                TotalAmount = total,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrderItems = orderItems
            };

            // Remove checked-out items from cart
            lock (cart)
            {
                cart.RemoveAll(i => selectedCartItemIds.Contains(i.CartItemId));
            }

            // Store order in local list
            var customerOrders = _localCustomerOrders.GetOrAdd(customerId, _ => new List<OrderDto>());
            lock (customerOrders)
            {
                customerOrders.Insert(0, newOrder);
            }

            // Sync with staff order dashboard model if available
            return new OrderCheckoutResultDto
            {
                Order = newOrder,
                Subtotal = subtotal,
                Discount = discount,
                Shipping = shipping,
                Total = total,
                PaymentStatus = "Unpaid (COD / Pending)"
            };
        }

        public async Task<TransferResult> TransferOrderToCartAsync(int customerId, Guid orderId, CancellationToken cancellationToken = default)
        {
            var result = new TransferResult();

            // Try to find order from API or local
            OrderDto? targetOrder = null;
            try
            {
                var apiOrders = await _customerOrderApi.GetMyOrdersAsync(customerId, cancellationToken);
                targetOrder = apiOrders.FirstOrDefault(o => o.OrderId == orderId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "API get orders failed, looking in local orders");
            }

            if (targetOrder == null && _localCustomerOrders.TryGetValue(customerId, out var localList))
            {
                lock (localList)
                {
                    targetOrder = localList.FirstOrDefault(o => o.OrderId == orderId);
                }
            }

            if (targetOrder == null)
            {
                result.Success = false;
                result.Errors.Add("Order not found.");
                return result;
            }

            result.Total = targetOrder.OrderItems.Count;
            int transferred = 0;

            foreach (var item in targetOrder.OrderItems)
            {
                try
                {
                    await AddToCartAsync(customerId, item.ProductVariantId, item.Quantity, cancellationToken);
                    transferred++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"{item.ProductName}: {ex.Message}");
                }
            }

            result.Transferred = transferred;
            result.Success = transferred > 0;
            return result;
        }

        public async Task<ProductCustomerModel?> GetProductWithVariantsAsync(int productId, CancellationToken cancellationToken = default)
        {
            var all = await GetCustomerProductsAsync(null, null, cancellationToken);
            return all.FirstOrDefault(p => p.ProductId == productId);
        }

        public async Task<List<ProductCustomerModel>> GetCustomerProductsAsync(string? query = null, string? category = null, CancellationToken cancellationToken = default)
        {
            var result = new List<ProductCustomerModel>();

            try
            {
                var products = await _productApi.GetAllAsync(cancellationToken);
                var activeProducts = products.Where(p => !p.IsDeleted).ToList();

                foreach (var prod in activeProducts)
                {
                    List<ProductVariantModel> variants = new();
                    try
                    {
                        variants = await _variantApi.GetByProductAsync(prod.ProductId, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not fetch variants for product {ProductId}", prod.ProductId);
                    }

                    result.Add(new ProductCustomerModel
                    {
                        ProductId = prod.ProductId,
                        ProductName = prod.ProductName,
                        Brand = prod.Brand,
                        CategoryId = prod.CategoryId,
                        CategoryName = prod.CategoryName,
                        ImageUrl = prod.ImageUrl ?? (variants.FirstOrDefault()?.ImageUrl),
                        CreatedAt = prod.CreatedAt,
                        UpdatedAt = prod.UpdatedAt,
                        Variants = variants
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not fetch products from API, using catalog seed fallback");
            }

            // If API returned 0 products (e.g. backend is offline), load high-quality catalogue
            if (result.Count == 0)
            {
                result = GetSeedCatalogue();
            }

            // Filtering
            var filtered = result.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(query))
            {
                filtered = filtered.Where(p =>
                    p.ProductName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    p.Brand.Contains(query, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(category))
            {
                filtered = filtered.Where(p =>
                    p.CategoryName.Contains(category, StringComparison.OrdinalIgnoreCase));
            }

            return filtered.ToList();
        }

        public static List<OrderDto> GetLocalOrdersForCustomer(int customerId)
        {
            if (_localCustomerOrders.TryGetValue(customerId, out var list))
            {
                lock (list)
                {
                    return list.Select(CloneOrder).ToList();
                }
            }
            return new List<OrderDto>();
        }

        public static OrderDashboardModel GetDashboardSummary()
        {
            int totalOrders = 0;
            decimal totalRevenue = 0m;
            int pendingOrders = 0;

            foreach (var pair in _localCustomerOrders)
            {
                lock (pair.Value)
                {
                    totalOrders += pair.Value.Count;
                    totalRevenue += pair.Value.Sum(o => o.TotalAmount);
                    pendingOrders += pair.Value.Count(o => o.Status == "Pending");
                }
            }

            return new OrderDashboardModel
            {
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                PendingOrders = pendingOrders
            };
        }

        private async Task<CartItemProductVariantDto?> FindVariantAsync(Guid variantId, CancellationToken cancellationToken)
        {
            // 1. Try API variant
            try
            {
                var v = await _variantApi.GetAsync(variantId, cancellationToken);
                if (v != null)
                {
                    var prod = await _productApi.GetByIdAsync(v.ProductId, cancellationToken);
                    return new CartItemProductVariantDto
                    {
                        ProductVariantId = v.ProductVariantId,
                        ProductId = v.ProductId,
                        ProductName = prod?.ProductName ?? "Electronic Device",
                        Brand = prod?.Brand ?? "Brand",
                        Sku = v.Sku,
                        Color = v.Color,
                        Cpu = v.Cpu,
                        Ram = v.Ram,
                        Storage = v.Storage,
                        ScreenSize = v.ScreenSize,
                        Price = v.Price,
                        OriginalPrice = v.Price,
                        StockQuantity = v.StockQuantity,
                        ImageUrl = v.ImageUrl
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "API variant lookup failed, checking catalog");
            }

            // 2. Check catalog seed
            foreach (var p in GetSeedCatalogue())
            {
                var match = p.Variants.FirstOrDefault(v => v.ProductVariantId == variantId);
                if (match != null)
                {
                    return new CartItemProductVariantDto
                    {
                        ProductVariantId = match.ProductVariantId,
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        Brand = p.Brand,
                        Sku = match.Sku,
                        Color = match.Color,
                        Cpu = match.Cpu,
                        Ram = match.Ram,
                        Storage = match.Storage,
                        ScreenSize = match.ScreenSize,
                        Price = match.Price,
                        OriginalPrice = match.Price,
                        StockQuantity = match.StockQuantity,
                        ImageUrl = match.ImageUrl
                    };
                }
            }

            return null;
        }

        private static CartItemDto CloneCartItem(CartItemDto src)
        {
            return new CartItemDto
            {
                CartItemId = src.CartItemId,
                CustomerId = src.CustomerId,
                ProductVariantId = src.ProductVariantId,
                Quantity = src.Quantity,
                ProductVariant = src.ProductVariant == null ? null : new CartItemProductVariantDto
                {
                    ProductVariantId = src.ProductVariant.ProductVariantId,
                    ProductId = src.ProductVariant.ProductId,
                    Sku = src.ProductVariant.Sku,
                    Color = src.ProductVariant.Color,
                    Cpu = src.ProductVariant.Cpu,
                    Ram = src.ProductVariant.Ram,
                    Storage = src.ProductVariant.Storage,
                    ScreenSize = src.ProductVariant.ScreenSize,
                    Price = src.ProductVariant.Price,
                    OriginalPrice = src.ProductVariant.OriginalPrice,
                    StockQuantity = src.ProductVariant.StockQuantity,
                    ImageUrl = src.ProductVariant.ImageUrl,
                    ProductName = src.ProductVariant.ProductName,
                    Brand = src.ProductVariant.Brand
                }
            };
        }

        private static OrderDto CloneOrder(OrderDto src)
        {
            return new OrderDto
            {
                OrderId = src.OrderId,
                CustomerId = src.CustomerId,
                CustomerName = src.CustomerName,
                TotalAmount = src.TotalAmount,
                Status = src.Status,
                CreatedAt = src.CreatedAt,
                UpdatedAt = src.UpdatedAt,
                OrderItems = src.OrderItems.Select(oi => new OrderItemDto
                {
                    OrderItemId = oi.OrderItemId,
                    OrderId = oi.OrderId,
                    ProductVariantId = oi.ProductVariantId,
                    ProductName = oi.ProductName,
                    ProductVariantDetails = oi.ProductVariantDetails,
                    Quantity = oi.Quantity,
                    PriceAtPurchase = oi.PriceAtPurchase,
                    CreatedAt = oi.CreatedAt,
                    UpdatedAt = oi.UpdatedAt
                }).ToList()
            };
        }

        private static List<ProductCustomerModel> GetSeedCatalogue()
        {
            var p1Id = 1;
            var p2Id = 2;
            var p3Id = 3;
            var p4Id = 4;
            var p5Id = 5;
            var p6Id = 6;

            return new List<ProductCustomerModel>
            {
                new ProductCustomerModel
                {
                    ProductId = p1Id,
                    ProductName = "XtremeBook 14 Pro",
                    Brand = "ElectroNexus",
                    CategoryId = 1,
                    CategoryName = "Laptops",
                    ImageUrl = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=500&auto=format&fit=crop&q=80",
                    Variants = new List<ProductVariantModel>
                    {
                        new() { ProductVariantId = Guid.Parse("11111111-1111-1111-1111-111111111111"), ProductId = p1Id, Sku = "XB14-SLV-I5", Color = "Silver", Cpu = "Intel i5-13500H", Ram = "16GB", Storage = "512GB SSD", ScreenSize = "14\" 2.8K OLED", Price = 18990000, StockQuantity = 25, ImageUrl = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=500&auto=format&fit=crop&q=80" },
                        new() { ProductVariantId = Guid.Parse("11111111-1111-1111-1111-111111111112"), ProductId = p1Id, Sku = "XB14-BLK-I7", Color = "Space Black", Cpu = "Intel i7-13700H", Ram = "32GB", Storage = "1TB SSD", ScreenSize = "14\" 2.8K OLED", Price = 24990000, StockQuantity = 15, ImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=500&auto=format&fit=crop&q=80" }
                    }
                },
                new ProductCustomerModel
                {
                    ProductId = p2Id,
                    ProductName = "PocketPhone Pro Max",
                    Brand = "ElectroNexus",
                    CategoryId = 2,
                    CategoryName = "Phones",
                    ImageUrl = "https://images.unsplash.com/photo-1592750475338-74b7b21085ab?w=500&auto=format&fit=crop&q=80",
                    Variants = new List<ProductVariantModel>
                    {
                        new() { ProductVariantId = Guid.Parse("22222222-2222-2222-2222-222222222221"), ProductId = p2Id, Sku = "PPP-TITAN-256", Color = "Natural Titanium", Cpu = "A17 Pro Hexa-core", Ram = "8GB", Storage = "256GB", ScreenSize = "6.7\" 120Hz OLED", Price = 27990000, StockQuantity = 40, ImageUrl = "https://images.unsplash.com/photo-1592750475338-74b7b21085ab?w=500&auto=format&fit=crop&q=80" },
                        new() { ProductVariantId = Guid.Parse("22222222-2222-2222-2222-222222222222"), ProductId = p2Id, Sku = "PPP-BLUE-512", Color = "Deep Blue", Cpu = "A17 Pro Hexa-core", Ram = "8GB", Storage = "512GB", ScreenSize = "6.7\" 120Hz OLED", Price = 32490000, StockQuantity = 20, ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02560?w=500&auto=format&fit=crop&q=80" }
                    }
                },
                new ProductCustomerModel
                {
                    ProductId = p3Id,
                    ProductName = "UltraBook Air 13",
                    Brand = "ProTech",
                    CategoryId = 1,
                    CategoryName = "Laptops",
                    ImageUrl = "https://images.unsplash.com/photo-1541807084-5c52b6b3adef?w=500&auto=format&fit=crop&q=80",
                    Variants = new List<ProductVariantModel>
                    {
                        new() { ProductVariantId = Guid.Parse("33333333-3333-3333-3333-333333333331"), ProductId = p3Id, Sku = "UBA13-SILVER", Color = "Starlight", Cpu = "Apple M3 Chip", Ram = "16GB", Storage = "512GB SSD", ScreenSize = "13.6\" Liquid Retina", Price = 28990000, StockQuantity = 30, ImageUrl = "https://images.unsplash.com/photo-1541807084-5c52b6b3adef?w=500&auto=format&fit=crop&q=80" }
                    }
                },
                new ProductCustomerModel
                {
                    ProductId = p4Id,
                    ProductName = "TabPro 11 Studio",
                    Brand = "ElectroNexus",
                    CategoryId = 3,
                    CategoryName = "Tablets",
                    ImageUrl = "https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?w=500&auto=format&fit=crop&q=80",
                    Variants = new List<ProductVariantModel>
                    {
                        new() { ProductVariantId = Guid.Parse("44444444-4444-4444-4444-444444444441"), ProductId = p4Id, Sku = "TAB11-GRAY-128", Color = "Space Gray", Cpu = "ARM Octa-Core Pro", Ram = "8GB", Storage = "128GB", ScreenSize = "11\" 120Hz ProMotion", Price = 15490000, StockQuantity = 35, ImageUrl = "https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?w=500&auto=format&fit=crop&q=80" },
                        new() { ProductVariantId = Guid.Parse("44444444-4444-4444-4444-444444444442"), ProductId = p4Id, Sku = "TAB11-SLV-256", Color = "Silver", Cpu = "ARM Octa-Core Pro", Ram = "8GB", Storage = "256GB", ScreenSize = "11\" 120Hz ProMotion", Price = 17990000, StockQuantity = 18, ImageUrl = "https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?w=500&auto=format&fit=crop&q=80" }
                    }
                },
                new ProductCustomerModel
                {
                    ProductId = p5Id,
                    ProductName = "SoundBuds Pro ANC",
                    Brand = "AudioTech",
                    CategoryId = 4,
                    CategoryName = "Accessories",
                    ImageUrl = "https://images.unsplash.com/photo-1590658268037-6bf12165a8df?w=500&auto=format&fit=crop&q=80",
                    Variants = new List<ProductVariantModel>
                    {
                        new() { ProductVariantId = Guid.Parse("55555555-5555-5555-5555-555555555551"), ProductId = p5Id, Sku = "SB-ANC-WHT", Color = "Ceramic White", Cpu = "H2 Audio Chip", Ram = null, Storage = null, ScreenSize = null, Price = 4990000, StockQuantity = 50, ImageUrl = "https://images.unsplash.com/photo-1590658268037-6bf12165a8df?w=500&auto=format&fit=crop&q=80" },
                        new() { ProductVariantId = Guid.Parse("55555555-5555-5555-5555-555555555552"), ProductId = p5Id, Sku = "SB-ANC-BLK", Color = "Midnight Black", Cpu = "H2 Audio Chip", Ram = null, Storage = null, ScreenSize = null, Price = 4990000, StockQuantity = 45, ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=500&auto=format&fit=crop&q=80" }
                    }
                },
                new ProductCustomerModel
                {
                    ProductId = p6Id,
                    ProductName = "FastCharge MagSafe 100W",
                    Brand = "ChargeMaster",
                    CategoryId = 4,
                    CategoryName = "Accessories",
                    ImageUrl = "https://images.unsplash.com/photo-1622445262464-84b1456045b6?w=500&auto=format&fit=crop&q=80",
                    Variants = new List<ProductVariantModel>
                    {
                        new() { ProductVariantId = Guid.Parse("66666666-6666-6666-6666-666666666661"), ProductId = p6Id, Sku = "FC100-WHT", Color = "Pure White", Cpu = null, Ram = null, Storage = null, ScreenSize = null, Price = 1290000, StockQuantity = 80, ImageUrl = "https://images.unsplash.com/photo-1622445262464-84b1456045b6?w=500&auto=format&fit=crop&q=80" }
                    }
                }
            };
        }
    }
}
