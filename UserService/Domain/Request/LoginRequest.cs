using UserService.Domain.Entities;

namespace UserService.Domain.Request
{
    public record LoginRequest(
     string Email,
     string Password);
}
