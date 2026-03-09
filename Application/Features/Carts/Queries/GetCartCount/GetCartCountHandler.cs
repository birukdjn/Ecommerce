using Domain.Common;
using MediatR;

namespace Application.Features.Carts.Queries.GetCartCount
{
    public record GetCartCountQuery : IRequest<Result<int>>;
}