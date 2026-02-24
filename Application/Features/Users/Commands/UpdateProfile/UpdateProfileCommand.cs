using Domain.Common;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Features.Profile.Commands.UpdateProfile
{
    public class UpdateProfileCommand:IRequest<Result<Guid>>
    {
        public string? FullName { get; set; }
        [Phone]
        public string? PhoneNumber { get; set; }

    }
}
