using System.ComponentModel.DataAnnotations;

namespace Application.Features.Users.Commands.Identity
{

    public record RegisterCommand
    (
        [EmailAddress]
        string Email,
        string Password,
        string FullName,
        [Phone]
        string PhoneNumber,
        string? ProfilePictureUrl
    );
}