using System.ComponentModel.DataAnnotations;

namespace Application.Features.Users.Commands.Identity
{
    public record LoginCommand
    (
        [Required]
        string PhoneOrEmail,
        [Required]
        string Password,
        bool RememberMe,
        string? TwoFactorCode = null,
        string? TwoFactorRecoveryCode = null
    );
}
