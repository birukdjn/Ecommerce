using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace Application.Features.Admins.Commands.ApproveVendor
{
    public class ApproveVendorCommandHandler(
        IApplicationDbContext context,
        UserManager<ApplicationUser> userManager) : IRequestHandler<ApproveVendorCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(ApproveVendorCommand request, CancellationToken cancellationToken)
        {
            var vendor = await context.Vendors
                .IgnoreQueryFilters()
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == request.VendorId, cancellationToken);

            if (vendor == null)
                return Result<bool>.Failure("Vendor request not found.");

            if (vendor.Status == VendorStatus.Active)
                return Result<bool>.Failure("Vendor is already active.");


            vendor.Status = VendorStatus.Active;

            if (vendor.User != null)
            {
                var isInRole = await userManager.IsInRoleAsync(vendor.User, Roles.Vendor);
                if (!isInRole)
                {
                    await userManager.AddToRoleAsync(vendor.User, Roles.Vendor);
                    await userManager.UpdateSecurityStampAsync(vendor.User);
                }
            }

            await context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
