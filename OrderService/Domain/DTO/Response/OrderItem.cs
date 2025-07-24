using OrderService.Domain.Entities;
using OrderService.Infrastructure.DTO;

namespace OrderService.Domain.DTO.Response
{
    public record OrderItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public float TotalPrice { get; set; }
    }
}
