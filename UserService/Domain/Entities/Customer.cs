using System.Reflection.Metadata;

namespace UserService.Domain.Entities
{
    public class Customer
    {
        public int ID { get; set; }      
        public required string Name { get; set; } 
        public required string Phone { get; set; }
        public required string Address { get; set; }
        public required string Gender { get; set; }

        public int? AccountID { get; set; } 
        public Account? Account { get; set; } 
    }
}
