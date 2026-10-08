namespace PresentationMVC.Models.Order.Staff
{
    public class OrderDashboardModel
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
    }
}
