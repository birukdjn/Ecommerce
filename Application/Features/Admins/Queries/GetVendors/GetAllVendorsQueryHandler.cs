using Application.DTOs;
using Domain.Common;
using Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace Application.Features.Admins.Queries.GetVendors
{
    public class GetAllVendorsHandler(IApplicationDbContext context)
        : IRequestHandler<GetAllVendorsQuery, Result<List<VendorSummaryDto>>>
    {
        public async Task<Result<List<VendorSummaryDto>>> Handle(GetAllVendorsQuery request, CancellationToken ct)
        {
            var vendors = await context.Users
                .Where(u => u.Vendor != null)
                .Select(u => new VendorSummaryDto(
                    u.Id,
                    u.FullName ?? "N/A",
                    u.Vendor!.StoreName ?? "N/A",
                    u.Email ?? "N/A",
                    u.PhoneNumber ?? "N/A",
                    u.Vendor.Status.ToString(),
                    u.Vendor.Products.Count,
                    u.CreatedAt
                ))
                .ToListAsync(ct);
            return Result<List<VendorSummaryDto>>.Success(vendors);
        }
    }
}