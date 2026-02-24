namespace Application.Features.Users.Commands.Identity
{
    public record LoginCommand
    (
        string PhoneOrEmail,
        string Password,
        bool RememberMe,
        string? TwoFactorCode = null,
        string? TwoFactorRecoveryCode = null
    );
}
