namespace UserService.Domain.DTO
{
    public record EmailValidatorDTO(
   bool IsValid,
   string Message);
}
