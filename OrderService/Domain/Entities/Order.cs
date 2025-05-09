namespace OrderService.Domain.Entities
{
    public class Order
    {
        public int ID { get; set; }
        public int CustomerID { get; set; }
        public DateTime OrderDate { get; set; }
        public float Total { get; set; }
        public string Status { get; set; } = "Pending";
    }
}
