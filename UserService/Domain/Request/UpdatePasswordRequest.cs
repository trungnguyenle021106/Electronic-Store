namespace UserService.Domain.Request
{
    public record UpdatePasswordRequest
    (string oldPassword, string newPassword);
}
