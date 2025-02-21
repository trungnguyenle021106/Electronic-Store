namespace UserService.Domain.Entities
{
    public class Account
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool Role { get; set; }
        public bool Status { get; set; }
        public Customer Customer { get; set; }
    }
}
