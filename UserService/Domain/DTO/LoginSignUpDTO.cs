using UserService.Domain.Entities;

namespace UserService.Domain.DTO
{
    public record LoginSignUpDTO
    (
        Account Account,
        string AccessToken,
        RefreshToken RefreshToken
    );
}
