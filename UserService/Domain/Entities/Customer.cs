namespace UserService.Domain.Entities
{
    public class Customer
    {
        public int ID { get; set; }
        public int? AccountID { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public Account Account { get; set; }
    }
}
