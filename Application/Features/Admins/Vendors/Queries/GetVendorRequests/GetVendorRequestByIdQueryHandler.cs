using Application.DTOs;
using Domain.Common;
using Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admins.Queries.GetVendorRequests
{
    public class GetVendorRequestByIdQueryHandler(IApplicationDbContext context)
            : IRequestHandler<GetVendorRequestByIdQuery, Result<VendorRequestDetailsDto>>
    {
        public async Task<Result<VendorRequestDetailsDto>> Handle(GetVendorRequestByIdQuery request, CancellationToken ct)
        {
            var vendor = await context.Vendors
                .IgnoreQueryFilters()
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == request.Id, ct);

            if (vendor == null) return Result<VendorRequestDetailsDto>.Failure("Request not found");

            var dto = new VendorRequestDetailsDto(
                vendor.Id,
                vendor.StoreName,
                vendor.Description,
                vendor.LicenseUrl,
                vendor.LogoUrl,
                vendor.RejectionReason, 
                vendor.User.FullName ?? "Unknown",
                vendor.User.Email ?? "Unknown",
                vendor.Status.ToString(),
                vendor.CreatedAt);

            return Result<VendorRequestDetailsDto>.Success(dto);
        }
    }
}