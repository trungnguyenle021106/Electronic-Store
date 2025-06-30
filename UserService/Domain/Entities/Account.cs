
using Microsoft.Extensions.Hosting;

namespace UserService.Domain.Entities
{
    public class Account
    {
        public int ID { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; }
        public required string Status { get; set; }

        public Customer? Customer { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; } = new List<RefreshToken>();
    }
}
