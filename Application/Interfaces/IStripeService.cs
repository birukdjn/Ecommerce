using Domain.Entities;
namespace Application.Interfaces
{
    public interface IStripeService
    {
        Task<string> CreateCheckoutSessionAsync(Order order);
        Task<Stripe.Checkout.Session?> GetSessionAsync(string sessionId);
    }
}