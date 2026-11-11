namespace PresentationMVC.Models.Order.Customer
{
    public class OrderCheckoutResultDto
    {
        public OrderDto Order { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Shipping { get; set; }
        public decimal Total { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
    }
}
