using MediatR;
using Domain.Common;

namespace Application.Features.Orders.Commands.Checkout
{

    public record CheckoutCommand(
        Guid? AddressId,
        string? ShippingFullName = null,
        string? ShippingPhoneNumber = null,
        string? ShippingCountry = null,
        string? ShippingRegion = null,
        string? ShippingCity = null,
        string? ShippingSpecialPlaceName = null) : IRequest<Result<Guid>>;
}