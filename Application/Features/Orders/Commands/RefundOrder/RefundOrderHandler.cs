using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Commands.RefundOrder
{
    public class RefundOrderHandler(IUnitOfWork unitOfWork) : IRequestHandler<RefundOrderCommand, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(RefundOrderCommand command, CancellationToken cancellationToken)
        {
            var subOrder = await unitOfWork.Repository<SubOrder>().Query()
                .Include(s => s.Vendor).ThenInclude(v => v.Wallet)
                .FirstOrDefaultAsync(s => s.Id == command.SubOrderId);

            if (subOrder == null) return Result<Unit>.Failure("Order not found.");

            decimal vendorEarnings = subOrder.SubTotal - subOrder.PlatformCommission;

            var wallet = subOrder.Vendor.Wallet;
            if (wallet != null)
            {
                wallet.Balance -= vendorEarnings;

                var refundEntry = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    Amount = vendorEarnings,
                    Type = TransactionType.Debit,
                    Description = $"Refund processed for SubOrder {subOrder.Id}"
                };
                unitOfWork.Repository<WalletTransaction>().Add(refundEntry);
            }

            subOrder.Status = SubOrderStatus.Refunded;

            await unitOfWork.Complete();
            return Result<Unit>.Success(Unit.Value);
        }
    }
}