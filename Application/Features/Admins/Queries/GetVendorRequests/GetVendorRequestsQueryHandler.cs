using Application.DTOs.Admin;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admins.Queries.GetVendorRequests
{
    public class GetVendorRequestsHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetVendorRequestsQuery, Result<List<VendorRequestDetailsDto>>>
    {
        public async Task<Result<List<VendorRequestDetailsDto>>> Handle(GetVendorRequestsQuery request, CancellationToken ct)
        {
            if (!currentUserService.IsAdmin())
                return Result<List<VendorRequestDetailsDto>>.Failure("Unauthorized");

            var vendorRepo = unitOfWork.Repository<Vendor>();

            var requests = await vendorRepo.Query()
            .AsNoTracking()
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