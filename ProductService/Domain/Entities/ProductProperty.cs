namespace ProductService.Domain.Entities
{
    public class ProductProperty
    {
        public int ID { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<Product> Products { get; } = [];
    }
}
