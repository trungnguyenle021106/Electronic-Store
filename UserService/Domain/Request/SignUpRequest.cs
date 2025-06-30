namespace UserService.Domain.Request
{
    public record SignUpRequest
    (
     string Email,
     string Password,
     string Name,
     string Phone,
     string Address,
     string Gender
    );
}
