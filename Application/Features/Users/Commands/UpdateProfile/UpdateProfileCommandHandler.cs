
using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Profile.Commands.UpdateProfile
{
    public class UpdateProfileCommandHandler(
    UserManager<ApplicationUser> userManager,
    ICurrentUserService currentUserService
    ) : IRequestHandler<UpdateProfileCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();

            if (userId == null || userId ==Guid.Empty) return Result<Guid>.Failure("User not authenticated.");

            var user = await userManager.FindByIdAsync(userId.ToString()!);
            if (user == null) return Result<Guid>.Failure("User not found.");

            user.FullName = request.FullName;
            user.PhoneNumber = request.PhoneNumber;

            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var error = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<Guid>.Failure(error);
            }
            return Result<Guid>.Success(user.Id);
        }
    }
}