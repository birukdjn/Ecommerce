using Application.DTOs.Product;
using Domain.Common;
using MediatR;

namespace Application.Features.Products.Queries.GetMyProductDetail
{
    public record GetMyProductDetailQuery(Guid Id) : IRequest<Result<MyProductDetailDto>>;

}