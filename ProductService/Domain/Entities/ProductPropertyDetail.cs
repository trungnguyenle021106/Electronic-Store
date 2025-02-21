using System.Text.Json.Serialization;

namespace ProductService.Domain.Entities
{
    public class ProductPropertyDetail
    {
        public int ProductID { get; set; }
        public int ProductPropertyID { get; set; }
        [JsonIgnore]
        public Product Product { get; set; }
        [JsonIgnore]
        public ProductProperty ProductProperty { get; set; }
    }
}
