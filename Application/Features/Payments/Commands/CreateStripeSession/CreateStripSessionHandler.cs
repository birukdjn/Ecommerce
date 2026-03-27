using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Payments.Commands.CreateStripeSession
{
    public class CreateStripeSessionHandler(IUnitOfWork unitOfWork, IStripeService stripeService)
    : IRequestHandler<CreateStripeSessionCommand, Result<string>>
    {
        public async Task<Result<string>> Handle(CreateStripeSessionCommand request, CancellationToken cancellationToken)
        {
            var order = await unitOfWork.Repository<Order>().Query()
                .Include(o => o.SubOrders)
                    .ThenInclude(so => so.Items)
                        .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null) return Result<string>.Failure("Order not found");

            var checkoutUrl = await stripeService.CreateCheckoutSessionAsync(order);

            return Result<string>.Success(checkoutUrl);
        }
    }
}