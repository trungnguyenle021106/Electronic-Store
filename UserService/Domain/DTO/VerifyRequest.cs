namespace UserService.Domain.DTO
{
    public class VerifyRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Code { get; set; }
    }
}
