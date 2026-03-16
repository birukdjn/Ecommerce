using Application.Interfaces;
using Domain.Common;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace Application.Features.Admins.Commands.RejectVendor
{
    public class RejectVendorHandler(IApplicationDbContext context)
        : IRequestHandler<RejectVendorCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(RejectVendorCommand command, CancellationToken cancellationToken)
        {
            var vendor = await context.Vendors
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(v => v.Id == command.VendorId, cancellationToken);

            if (vendor == null) return Result<bool>.Failure("Vendor not found.");

            vendor.Status = VendorStatus.Rejected;
            vendor.RejectionReason = command.Reason;

            await context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}