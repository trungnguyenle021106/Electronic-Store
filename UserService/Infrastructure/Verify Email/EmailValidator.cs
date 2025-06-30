using UserService.Domain.Response;
using Verifalia.Api;
using Verifalia.Api.EmailValidations.Models;

namespace UserService.Infrastructure.Verify_Email
{
    public class EmailValidator
    {
        public async Task<EmailValidatorDTO> CheckEmailValid(string email)
        {
            var verifalia = new VerifaliaRestClient("f4edf41478a24f04ac5e1ceaa1fb7163", "0937210476Nn");
            var job = await verifalia.EmailValidations.SubmitAsync(email);

            var entry = job.Entries[0];

            if (entry.Classification == ValidationEntryClassification.Deliverable)
            {
                return new EmailValidatorDTO(true, "Email hợp lệ");
            }

            return new EmailValidatorDTO(false, "Email không hợp lệ");
        }
    }
}
