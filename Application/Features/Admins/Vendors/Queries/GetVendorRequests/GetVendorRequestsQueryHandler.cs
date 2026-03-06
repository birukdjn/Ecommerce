using Application.DTOs;
using Domain.Common;
using Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admins.Queries.GetVendorRequests
{
    public class GetVendorRequestsHandler(IApplicationDbContext context)
        : IRequestHandler<GetVendorRequestsQuery, Result<List<VendorRequestDetailsDto>>>
    {
        public async Task<Result<List<VendorRequestDetailsDto>>> Handle(GetVendorRequestsQuery request, CancellationToken ct)
        {
            var requests = await context.Vendors
            .IgnoreQueryFilters()
            .Include(v => v.User)
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new VendorRequestDetailsDto(
                v.Id,
                v.StoreName,
                v.Description,
                v.LicenseUrl,
                v.LogoUrl,
                v.RejectionReason,
                v.User.FullName ?? "Unknown",
                v.User.Email ?? "Unknown",
                v.Status.ToString(),
                v.CreatedAt))
            .ToListAsync(ct);

            return Result<List<VendorRequestDetailsDto>>.Success(requests);
        }
    }
}