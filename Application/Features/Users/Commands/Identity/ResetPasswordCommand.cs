using System.ComponentModel.DataAnnotations;

namespace Application.Features.Users.Commands.Identity
{
    public record ResetPasswordCommand
    (
        [Phone]
        string PhoneNumber,
        string ResetCode,
        string NewPassword
    );
}