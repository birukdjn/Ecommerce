using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Vendors.Commands.BecomeVendor
{
    public class BecomeVendorCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService) : IRequestHandler<BecomeVendorCommand, Result<Guid>>
    {

        public async Task<Result<Guid>> Handle(BecomeVendorCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();

            if (userId == null || userId == Guid.Empty)
                return Result<Guid>.Failure("User not authenticated.");
            
            var user = await context.Users
                .Include(u => u.Vendor)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);


            if (user == null)
                return Result<Guid>.Failure("User not found.");

            
            if (user.Vendor != null)
            {
                if (user.Vendor.Status == VendorStatus.Active)
                    return Result<Guid>.Failure("A vendor account already exists for this user.");

                if (user.Vendor.Status == VendorStatus.Pending)
                    return Result<Guid>.Failure("User already has a pending vendor request.");
            }


            using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                
                var vendor = new Vendor
                {
                    Id = Guid.NewGuid(),
                    UserId = userId.Value,
                    StoreName = request.StoreName,
                    Description = request.Description,
                    LogoUrl = request.LogoUrl,
                    Status = VendorStatus.Pending,
                    LicenseUrl = request.LicenseUrl,
                    
                    
                };

                var wallet = new VendorWallet
                {
                    Id = Guid.NewGuid(),
                    VendorId = vendor.Id,
                    
                };
                context.Vendors.Add(vendor);
                context.VendorWallets.Add(wallet);

                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result<Guid>.Success(vendor.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<Guid>.Failure($"Failed to create vendor: {ex.Message}");
            }

        }
    }
}
