using Application.DTOs.Admin;
using Application.Interfaces;
using Domain.Common;
using Domain.Constants;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace Application.Features.Admins.Queries.GetUsers
{
    public class GetAllUsersHandler(
UserManager<ApplicationUser> userManager,
ICurrentUserService currentUserService)
: IRequestHandler<GetAllUsersQuery, Result<List<UserSummaryDto>>>
    {
        public async Task<Result<List<UserSummaryDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            if (!currentUserService.IsAdmin())
                return Result<List<UserSummaryDto>>.Failure("unauthorized");

            var users = await userManager.GetUsersInRoleAsync(Roles.Customer);

            var result = users.Select(u => new UserSummaryDto(
                u.Id,
                u.FullName ?? "N/A",
                u.Email ?? "N/A",
                u.PhoneNumber ?? "N/A",
                u.IsActive ? "Active" : "Inactive",
                u.CreatedAt
            )).ToList();

            return Result<List<UserSummaryDto>>.Success(result);
        }
    }
}