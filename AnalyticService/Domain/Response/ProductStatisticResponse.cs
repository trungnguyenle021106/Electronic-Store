using AnalyticService.Infrastructure.DTO;

namespace AnalyticService.Domain.Response
{
    public class ProductStatisticResponse
    {
        public Product Product { get; set; }
        public int TotalSales { get; set; }
    }
}
