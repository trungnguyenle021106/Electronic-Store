namespace AnalyticService.Infrastructure.DTO
{
    public class Product
    {
        public int ID { get; set; }
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public string Image { get; set; } = "";
        public string Description { get; set; } = "";
        public float Price { get; set; }
        public string Status { get; set; } = "";
        public int ProductTypeID { get; set; }
        public int ProductBrandID { get; set; }
    }
}
