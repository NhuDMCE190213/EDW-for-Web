using PresentationMVC.Models.Order.Customer;

namespace PresentationMVC.Models.Order.Customer
{
    public class MyOrdersViewModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public List<OrderDto> Orders { get; set; } = new();
    }
}
