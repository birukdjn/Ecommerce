using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace Application.Features.Payments.Commands.ConfirmPayment
{
    public class ConfirmPaymentHandler(
        IUnitOfWork unitOfWork,
        IStripeService stripeService,
        IApplicationDbContext context
    ) : IRequestHandler<ConfirmPaymentCommand, Result>
    {
        public async Task<Result> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
        {
            // Declare transaction outside try so it is visible in catch
            IDbContextTransaction? transaction = null;

            try
            {
                // 1️⃣ Verify payment with Stripe
                var session = await stripeService.GetSessionAsync(request.ProviderTransactionId);
                if (session == null || session.PaymentStatus != "paid")
                    return Result.Failure("Payment not verified");

                // 2️⃣ Idempotency check
                var existingTransaction = await unitOfWork.Repository<PaymentTransaction>()
                    .Query()
                    .FirstOrDefaultAsync(x => x.TransactionId == request.ProviderTransactionId, cancellationToken);

                if (existingTransaction?.Status == PaymentStatus.Paid)
                    return Result.Success();

                // 3️⃣ Load order with related SubOrders & Wallets
                var order = await unitOfWork.Repository<Order>()
                    .Query()
                    .Include(o => o.SubOrders)
                        .ThenInclude(so => so.Vendor)
                            .ThenInclude(v => v.Wallet)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

                if (order == null)
                    return Result.Failure("Order not found");

                if (order.PaymentStatus == PaymentStatus.Paid)
                    return Result.Success();

                // 4️⃣ Start EF Core transaction
                transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

                // 5️⃣ Update Order
                order.PaymentStatus = PaymentStatus.Paid;
                order.Status = OrderStatus.Processing;

                // 6️⃣ Distribute funds to Vendor Escrow
                foreach (var subOrder in order.SubOrders)
                {
                    var vendorShare = subOrder.SubTotal - subOrder.PlatformCommission;

                    var wallet = subOrder.Vendor.Wallet;
                    if (wallet == null)
                        return Result.Failure($"Wallet not found for vendor {subOrder.VendorId}");

                    wallet.EscrowBalance += vendorShare;

                    unitOfWork.Repository<WalletTransaction>().Add(new WalletTransaction
                    {
                        WalletId = wallet.Id,
                        Amount = vendorShare,
                        Type = TransactionType.Credit,
                        Description = $"Escrow funding (Stripe). Order: {order.OrderNumber}",
                        RelatedOrderId = order.Id
                    });
                }

                // 7️⃣ Record payment transaction
                if (existingTransaction == null)
                {
                    unitOfWork.Repository<PaymentTransaction>().Add(new PaymentTransaction
                    {
                        OrderId = order.Id,
                        TransactionId = request.ProviderTransactionId,
                        Amount = order.TotalAmount,
                        Status = PaymentStatus.Paid,
                        VerifiedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    existingTransaction.Status = PaymentStatus.Paid;
                    existingTransaction.VerifiedAt = DateTime.UtcNow;
                }

                // 8️⃣ Save changes
                await unitOfWork.Complete();

                // 9️⃣ Commit transaction
                await transaction.CommitAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                return Result.Failure($"Payment processing failed: {ex.Message}");
            }
        }
    }
}