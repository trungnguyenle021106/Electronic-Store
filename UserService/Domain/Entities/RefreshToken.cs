namespace UserService.Domain.Entities
{
    public class RefreshToken
    {
        public int ID { get; set; }
        public required string TokenHash { get; set; }
        public required bool IsRevoked { get; set; } = false;
        public required DateTime ExpiresAt { get; set; }
        public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTimeOffset? RevokedAt { get; set; } = null;

        public int? AccountID { get; set; }
        public Account? Account { get; set; }
    }
}
