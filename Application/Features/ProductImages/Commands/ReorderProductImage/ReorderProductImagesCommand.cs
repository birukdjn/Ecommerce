using Domain.Common;
using MediatR;

namespace Application.Features.ProductImages.Commands.ReorderProductImage
{
    public record ReorderProductImagesCommand(Guid ProductId, List<Guid> OrderedIds) : IRequest<Result<Unit>>;
}