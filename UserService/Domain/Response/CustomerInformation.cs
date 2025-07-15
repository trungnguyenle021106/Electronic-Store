namespace UserService.Domain.Response
{
    public record CustomerInformation
    (
        string Email,
        string Name,
        string Phone,
        string Address,
        string Gender
    );
}
