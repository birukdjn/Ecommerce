using Application.DTOs.Cart;
using Domain.Common;
using MediatR;

namespace Application.Features.Carts.Commands.ValidateCart
{

    public record ValidateCartCommand : IRequest<Result<CartValidationDto>>;
}