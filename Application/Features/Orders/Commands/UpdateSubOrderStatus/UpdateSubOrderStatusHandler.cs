using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Commands.UpdateSubOrderStatus
{
    public class UpdateSubOrderStatusHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<UpdateSubOrderStatusCommand, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(UpdateSubOrderStatusCommand command, CancellationToken cancellationToken)
        {
            if (!currentUserService.IsVendor())
                return Result<Unit>.Failure("Unauthorized");

            var vendorId = currentUserService.GetCurrentVendorId();
            var subOrderRepo = unitOfWork.Repository<SubOrder>();
            var vendorWalletRepo = unitOfWork.Repository<VendorWallet>();
            var walletTransactionRepo = unitOfWork.Repository<WalletTransaction>();


            var subOrder = await subOrderRepo.Query()
                .Include(s => s.MasterOrder)
                .FirstOrDefaultAsync(s => s.Id == command.SubOrderId && s.VendorId == vendorId, cancellationToken);

            if (subOrder == null) return Result<Unit>.Failure("Sub-order not found.");

            if (subOrder.Status == SubOrderStatus.Delivered && command.NewStatus == SubOrderStatus.Delivered)
                return Result<Unit>.Success(Unit.Value);

            subOrder.Status = command.NewStatus;
            subOrderRepo.Update(subOrder);

            if (command.NewStatus == SubOrderStatus.Delivered)
            {
                var wallet = await vendorWalletRepo.Query()
                    .FirstOrDefaultAsync(w => w.VendorId == vendorId, cancellationToken);

                if (wallet == null) return Result<Unit>.Failure("Vendor wallet not found.");

                decimal earnings = subOrder.SubTotal - subOrder.PlatformCommission;
                wallet.Balance += earnings;

                var transaction = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    Amount = earnings,
                    Type = TransactionType.Credit,
                    Description = $"Earnings from Order {subOrder.MasterOrder.OrderNumber}",
                    RelatedOrderId = subOrder.MasterOrder.Id
                };

                walletTransactionRepo.Add(transaction);
                vendorWalletRepo.Update(wallet);
            }

            var result = await unitOfWork.Complete();
            return result > 0 ? Result<Unit>.Success(Unit.Value) : Result<Unit>.Failure("Failed to update status.");
        }
    }
}