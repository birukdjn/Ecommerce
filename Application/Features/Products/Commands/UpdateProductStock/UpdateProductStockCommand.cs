using Domain.Common;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProductStock
{
    public record UpdateProductStockCommand(Guid Id, int NewQuantity) : IRequest<Result<bool>>;

}