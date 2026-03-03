using System.ComponentModel.DataAnnotations;

namespace Application.Features.Users.Commands.Identity
{
    public record ForgotPasswordCommand
   (
    [Required][Phone]
       string PhoneNumber
       );
}