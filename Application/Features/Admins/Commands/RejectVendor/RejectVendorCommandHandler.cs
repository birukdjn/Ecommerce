using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace Application.Features.Admins.Commands.RejectVendor
{
    public class RejectVendorCommandHandler(IApplicationDbContext context)
        : IRequestHandler<RejectVendorCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(RejectVendorCommand request, CancellationToken ct)
        {
            var vendor = await context.Vendors
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(v => v.Id == request.VendorId, ct);

            if (vendor == null) return Result<bool>.Failure("Vendor not found.");

            vendor.Status = VendorStatus.Rejected;
            vendor.RejectionReason = request.Reason;

            await context.SaveChangesAsync(ct);
            return Result<bool>.Success(true);
        }
    }
}