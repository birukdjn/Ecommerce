using Domain.Common;
using MediatR;

namespace Application.Features.ProductImages.Commands.SetPrimaryImage
{
    public record SetPrimaryImageCommand(Guid ProductId, Guid ImageId) : IRequest<Result<Unit>>;

}