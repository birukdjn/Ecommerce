using Application.Features.Payments.Commands.ConfirmPayment;
using Application.Features.Payments.Commands.CreateStripeSession;
using Infrastructure.Options;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe; // Add this back!
using Stripe.Checkout;

namespace Api.Controllers.Payment
{
    [ApiController]
    [Route("api/[controller]")]
    public class StripeController(IMediator mediator, IOptions<StripeOptions> stripeOptions) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly StripeOptions _stripeOptions = stripeOptions.Value;

        [HttpPost("initialize/{orderId}")]
        public async Task<IActionResult> InitializePayment(Guid orderId)
        {
            var result = await _mediator.Send(new CreateStripeSessionCommand(orderId));

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(new { checkoutUrl = result.Value });
        }



        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _stripeOptions.WebhookSecret
                );

                switch (stripeEvent.Type)
                {
                    case "checkout.session.completed":
                        var session = stripeEvent.Data.Object as Session;
                        var orderIdStr = session?.Metadata?["orderId"];

                        if (Guid.TryParse(orderIdStr, out var orderId))
                        {
                            await _mediator.Send(new ConfirmPaymentCommand(orderId, session!.Id));
                        }
                        break;

                    case "payment_intent.payment_failed":
                        break;
                }

                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest(new { error = e.Message });
            }
        }
    }
}