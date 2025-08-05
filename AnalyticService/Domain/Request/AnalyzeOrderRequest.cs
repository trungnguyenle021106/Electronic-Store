namespace AnalyticService.Domain.Request
{
    public class AnalyzeOrderRequest
    {
        public DateTime Date { get; set; }
        public float Total { get; set; }
        public int CancelledOrders { get; set; }
    }
}
