using Domain.Common.Interfaces;
using Application.DTOs.Wallet;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Wallets.Queries.GetVendorWallet;


public class GetVendorWalletHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<GetVendorWalletQuery, Result<VendorWalletDto>>
{
    public async Task<Result<VendorWalletDto>> Handle(GetVendorWalletQuery request, CancellationToken cancellationToken)
    {
        var vendorId = currentUserService.GetCurrentVendorId();
        if (vendorId == null) return Result<VendorWalletDto>.Failure("User is not a vendor.");

        // Fetch wallet with recent transactions
        var wallet = await unitOfWork.Repository<VendorWallet>().Query()
            .AsNoTracking()
            .Include(w => w.Vendor)
            .FirstOrDefaultAsync(w => w.VendorId == vendorId, cancellationToken);

        if (wallet == null) return Result<VendorWalletDto>.Failure("Wallet not found.");

        var transactions = await unitOfWork.Repository<WalletTransaction>().Query()
            .Where(t => t.WalletId == wallet.Id)
            .OrderByDescending(t => t.CreatedAt)
            .Take(20)
            .Select(t => new WalletTransactionDto(
                t.Id,
                t.Amount,
                t.Type.ToString(),
                t.Description,
                t.CreatedAt,
                t.RelatedOrderId
            ))
            .ToListAsync(cancellationToken);

        return Result<VendorWalletDto>.Success(new VendorWalletDto(
            wallet.Balance,
            wallet.EscrowBalance,
            transactions
        ));
    }
}