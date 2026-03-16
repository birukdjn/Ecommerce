using Application.DTOs.Admin;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admins.Queries.GetVendorRequestById
{
    public class GetVendorRequestByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
            : IRequestHandler<GetVendorRequestByIdQuery, Result<VendorRequestDetailsDto>>
    {
        public async Task<Result<VendorRequestDetailsDto>> Handle(GetVendorRequestByIdQuery request, CancellationToken cancellationToken)
        {
            if (!currentUserService.IsAdmin())
                return Result<VendorRequestDetailsDto>.Failure("Unauthorized");
            var vendorRepo = unitOfWork.Repository<Vendor>();

            var vendor = await vendorRepo.Query()
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

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