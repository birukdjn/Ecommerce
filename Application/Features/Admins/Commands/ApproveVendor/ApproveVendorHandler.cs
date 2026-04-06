using Application.Interfaces;
using Application.Templates.Email;
using Domain.Common;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace Application.Features.Admins.Commands.ApproveVendor
{
    public class ApproveVendorHandler(
        IApplicationDbContext context,
        IJobService jobService,
        ICurrentUserService currentUserService,
        UserManager<ApplicationUser> userManager) : IRequestHandler<ApproveVendorCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(ApproveVendorCommand command, CancellationToken cancellationToken)
        {
            if (!currentUserService.IsAdmin())
                return Result<bool>.Failure("Unauthorized");

            var vendor = await context.Vendors
                .IgnoreQueryFilters()
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == command.VendorId, cancellationToken);

            if (vendor == null)
                return Result<bool>.Failure("Vendor request not found.");

            if (vendor.Status == VendorStatus.Active)
                return Result<bool>.Failure("Vendor is already active.");


            vendor.Status = VendorStatus.Active;


            jobService.Enqueue<IEmailSender>(sender =>
                sender.SendEmailAsync(
                    vendor.User.Email!,
                    "Your Store is Live!",
                    EmailTemplates.GetVendorApprovedEmail(vendor.User.FullName!, vendor.StoreName)
                ));

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
