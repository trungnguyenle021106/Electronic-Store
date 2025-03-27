using UserService.Domain.Entities;

namespace UserService.Domain.Request
{
    public record LoginRequest(
     string UserName,
     string Password);
}
