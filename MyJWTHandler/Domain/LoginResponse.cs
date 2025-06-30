using UserService.Domain.Entities;

namespace UserService.Domain.Response
{
    public record LoginResponse(
    int statusCode,
    string Message,
    int? idAccount,
    string? role,
    Customer? customer);
}
