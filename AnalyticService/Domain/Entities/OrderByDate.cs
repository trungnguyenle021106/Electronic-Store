namespace AnalyticService.Domain.Entities
{
    public class OrderByDate
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int CancelledOrders { get; set; }
    }
}
