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
        private readonly IApplicationDbContext _context = context;
        private readonly ICurrentUserService _currentUserService = currentUserService;


        public async Task<Result<Guid>> Handle(BecomeVendorCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();

            if (userId == null || userId == Guid.Empty)
                return Result<Guid>.Failure("User not authenticated.");

            var existingVendorStatus = await _context.Vendors
                            .Where(v => v.UserId == userId)
                            .Select(v => v.Status)
                            .FirstOrDefaultAsync(cancellationToken);

            if (existingVendorStatus ==VendorStatus.Active)
                return Result<Guid>.Failure("A vendor account already exists for this user.");

           
            if(existingVendorStatus==VendorStatus.Pending)
                return Result<Guid>.Failure("User already has a pending vendor request.");


            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
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
                    CommissionRate = 0.10m,
                    CreatedAt = DateTime.UtcNow
                };

                var wallet = new VendorWallet
                {
                    Id = Guid.NewGuid(),
                    VendorId = vendor.Id,
                    Balance = 0,
                    EscrowBalance = 0,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Vendors.Add(vendor);
                _context.VendorWallets.Add(wallet);

                await _context.SaveChangesAsync(cancellationToken);
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
