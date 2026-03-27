using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Stripe;
using Stripe.Checkout;

namespace Infrastructure.Services
{
    public class StripeService(IUnitOfWork unitOfWork) : IStripeService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<string> CreateCheckoutSessionAsync(Order order)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },


                Metadata = new Dictionary<string, string>
                {
                    { "orderId", order.Id.ToString() }
                },


                ClientReferenceId = order.Id.ToString(),

                LineItems = order.SubOrders
                    .SelectMany(so => so.Items)
                    .Select(item => new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.UnitPrice * 100), // cents
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Name
                            }
                        },
                        Quantity = item.Quantity
                    }).ToList(),

                Mode = "payment",

                SuccessUrl = "https://yourfrontend.com/success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = "https://yourfrontend.com/cancel"
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            _unitOfWork.Repository<PaymentTransaction>().Add(new PaymentTransaction
            {
                OrderId = order.Id,
                TransactionId = session.Id,
                Amount = order.TotalAmount,
                Status = PaymentStatus.Pending
            });

            await _unitOfWork.Complete();
            return session.Url ?? throw new Exception("Stripe session URL not generated");
        }

        public async Task<Session?> GetSessionAsync(string sessionId)
        {
            try
            {
                var service = new SessionService();
                return await service.GetAsync(sessionId);
            }
            catch (StripeException)
            {
                return null;
            }
        }
    }
}
