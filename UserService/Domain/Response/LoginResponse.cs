using UserService.Domain.Entities;

namespace UserService.Domain.Response
{
    public record LoginResponse(
    int statusCode,
    string Message,
    int? idAccount,
    bool? role,
    string? AccessToken,
    Customer? customer);

}
