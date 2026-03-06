using Application.DTOs;
using Domain.Common;
using Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace Application.Features.Admins.Queries.GetUsers
{
    public class GetAllUsersHandler(IApplicationDbContext context)
        : IRequestHandler<GetAllUsersQuery, Result<List<UserSummaryDto>>>
    {
        public async Task<Result<List<UserSummaryDto>>> Handle(GetAllUsersQuery request, CancellationToken ct)
        {
            var users = await context.Users
                .Select(u => new UserSummaryDto(
                    u.Id,
                    u.FullName ?? "N/A",
                    u.Email ?? "N/A",
                    u.PhoneNumber ?? "N/A",
                    u.IsActive ? "Active" : "Inactive",
                    u.CreatedAt
                ))
                .ToListAsync(ct);

            return Result<List<UserSummaryDto>>.Success(users);
        }
    }
}