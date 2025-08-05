namespace UserService.Domain.Response
{
    public record CustomerInformation
    (
        int ID,
        string Email,
        string Name,
        string Phone,
        string Address,
        string Gender
    );
}
