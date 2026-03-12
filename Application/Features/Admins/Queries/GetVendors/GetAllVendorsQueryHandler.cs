using Application.DTOs.Admin;
using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace Application.Features.Admins.Queries.GetVendors
{
    public class GetAllVendorsHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetAllVendorsQuery, Result<List<VendorSummaryDto>>>
    {
        public async Task<Result<List<VendorSummaryDto>>> Handle(GetAllVendorsQuery request, CancellationToken ct)
        {
            if (!currentUserService.IsAdmin())
                return Result<List<VendorSummaryDto>>.Failure("Unauthorized");

            var vendorRepo = unitOfWork.Repository<Vendor>();
            var vendors = await vendorRepo.Query()
                .AsNoTracking()
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