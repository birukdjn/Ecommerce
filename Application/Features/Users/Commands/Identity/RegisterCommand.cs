namespace Application.Features.Users.Commands.Identity
{
    public record RegisterCommand
    (
        string Email,
        string Password,
        string FullName,
        string PhoneNumber,
        string? ProfilePictureUrl
    );
}