using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Wallets.Commands.RequestPayout
{

    public class RequestPayoutHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<RequestPayoutCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(RequestPayoutCommand command, CancellationToken cancellationToken)
        {
            var vendorId = currentUserService.GetCurrentVendorId();
            if (vendorId == null) return Result<Guid>.Failure("Unauthorized");

            if (command.Amount <= 0) return Result<Guid>.Failure("Amount must be greater than zero.");


            var wallet = await unitOfWork.Repository<VendorWallet>().Query()
                .FirstOrDefaultAsync(w => w.VendorId == vendorId, cancellationToken);

            if (wallet == null) return Result<Guid>.Failure("Wallet not found.");

            if (wallet.Balance < command.Amount)
                return Result<Guid>.Failure("Insufficient balance for this payout.");

            wallet.Balance -= command.Amount;

            var payoutRequest = new PayoutRequest
            {
                VendorId = vendorId.Value,
                Amount = command.Amount,
                Status = PayoutRequestStatus.Pending
            };

            var transaction = new WalletTransaction
            {
                WalletId = wallet.Id,
                Amount = command.Amount,
                Type = TransactionType.Debit,
                Description = "Payout request submitted",
            };

            unitOfWork.Repository<VendorWallet>().Update(wallet);
            unitOfWork.Repository<PayoutRequest>().Add(payoutRequest);
            unitOfWork.Repository<WalletTransaction>().Add(transaction);

            var result = await unitOfWork.Complete();

            return result > 0
                ? Result<Guid>.Success(payoutRequest.Id)
                : Result<Guid>.Failure("Failed to process payout request.");
        }
    }
}