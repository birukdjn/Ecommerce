using System.ComponentModel.DataAnnotations;

namespace Application.Features.Users.Commands.Identity
{
    public record VerifyPhoneCommand
    (
        [Phone]
        string PhoneNumber,
        string Code,
        string VerificationId
    );

}