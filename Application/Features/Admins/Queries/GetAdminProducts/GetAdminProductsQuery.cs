using Application.DTOs.Admin;
using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Queries.GetAdminProducts
{
    public record GetAdminProductsQuery(AdminProductSpecParams Params) : IRequest<Result<PagedList<AdminProductDto>>>;
}