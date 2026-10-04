using Microsoft.EntityFrameworkCore;
using DAL.Models;

namespace DAL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            
            // Explicitly set table names to match the existing migration snapshot
            modelBuilder.Entity<Category>().ToTable("categories");
            modelBuilder.Entity<Product>().ToTable("products");
            modelBuilder.Entity<ProductVariant>().ToTable("product_variants");
            modelBuilder.Entity<ProductReview>().ToTable("product_reviews");
            modelBuilder.Entity<Customer>().ToTable("customers");
            modelBuilder.Entity<CartItem>().ToTable("cart_items");
            
            // Orders and OrderItems also need explicit names because their configs use ToTable with constraints without names
            modelBuilder.Entity<Order>().ToTable("orders");
            modelBuilder.Entity<OrderItem>().ToTable("order_items");
        }
    }
}
