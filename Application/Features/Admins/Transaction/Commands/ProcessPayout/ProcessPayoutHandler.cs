using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admins.Transaction.Commands.ProcessPayout
{

    public class ProcessPayoutHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<ProcessPayoutCommand, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(ProcessPayoutCommand command, CancellationToken cancellationToken)
        {
            var payout = await unitOfWork.Repository<PayoutRequest>().Query()
                .Include(p => p.Vendor).ThenInclude(v => v.Wallet)
                .FirstOrDefaultAsync(p => p.Id == command.PayoutRequestId, cancellationToken);

            if (payout == null) return Result<Unit>.Failure("Payout request not found.");
            if (payout.Status != PayoutRequestStatus.Pending)
                return Result<Unit>.Failure("Request has already been processed.");

            if (command.Approved)
            {
                payout.Status = PayoutRequestStatus.Completed;
                payout.BankReference = command.BankReference;
                payout.ProcessedAt = DateTime.UtcNow;
            }
            else
            {
                payout.Status = PayoutRequestStatus.Failed;

                var wallet = payout.Vendor.Wallet;
                if (wallet != null)
                {
                    wallet.Balance += payout.Amount;

                    var refundTransaction = new WalletTransaction
                    {
                        WalletId = wallet.Id,
                        Amount = payout.Amount,
                        Type = TransactionType.Credit,
                        Description = $"Payout Rejected: {command.Note ?? "Invalid details"}"
                    };
                    unitOfWork.Repository<WalletTransaction>().Add(refundTransaction);
                    unitOfWork.Repository<VendorWallet>().Update(wallet);
                }
            }

            unitOfWork.Repository<PayoutRequest>().Update(payout);
            var result = await unitOfWork.Complete();

            return result > 0 ? Result<Unit>.Success(Unit.Value) : Result<Unit>.Failure("Failed to update payout.");
        }
    }
}