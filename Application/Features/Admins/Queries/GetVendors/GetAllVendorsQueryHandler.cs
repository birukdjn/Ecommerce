using Application.DTOs;
using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace Application.Features.Admins.Queries.GetVendors
{
    public class GetAllVendorsHandler(IApplicationDbContext context)
        : IRequestHandler<GetAllVendorsQuery, Result<List<VendorSummaryDto>>>
    {
        public async Task<Result<List<VendorSummaryDto>>> Handle(GetAllVendorsQuery request, CancellationToken ct)
        {
            var vendors = await context.Vendors
                .Select(v => new VendorSummaryDto(
                    v.Id,
                    v.User.FullName ?? "N/A",
                    v.StoreName ?? "N/A",
                    v.User.Email ?? "N/A",
                    v.User.PhoneNumber ?? "N/A",
                    v.Status.ToString(),
                    v.Products.Count,
                    v.CreatedAt

                ))
                .ToListAsync(ct);
            return Result<List<VendorSummaryDto>>.Success(vendors);
        }
    }
}