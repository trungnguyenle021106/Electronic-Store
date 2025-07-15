namespace ProductService.Domain.DTO
{
    public record ProductDTO
    {
        public int ID { get; set; }
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public string Image { get; set; } = "";
        public float Price { get; set; }
        public string Status { get; set; } = "";
        public string ProductTypeName { get; set; } = "";
        public string ProductBrandName { get; set; } = "";
    }
}
