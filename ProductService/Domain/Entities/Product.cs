using System.Text.Json.Serialization;

namespace ProductService.Domain.Entities
{
    public class Product
    {
        public int ID { get; set; }
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public string Image { get; set; } = "";
        public string Description { get; set; } = "";
        public float Price { get; set; }
        public bool Status { get; set; }
        public List<ProductProperty> ProductProperties { get; } = [];
    }
}
