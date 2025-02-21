using System.Text.Json.Serialization;

namespace OrderService.Domain.Entities
{
    public class OrderDetail
    {
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public float TotalPrice { get; set; }
        [JsonIgnore]
        public Order Order { get; set; }
    }
}
