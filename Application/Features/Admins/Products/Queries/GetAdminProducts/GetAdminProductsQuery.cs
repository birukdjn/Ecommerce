using Application.DTOs.Product.Admin;
using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Products.Queries.GetAdminProducts
{
    public record GetAdminProductsQuery(AdminProductSpecParams Params) : IRequest<Result<PagedList<AdminProductDto>>>;
}