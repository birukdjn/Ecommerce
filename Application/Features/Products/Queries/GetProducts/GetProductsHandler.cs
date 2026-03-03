using Application.DTOs.Product;
using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetProducts
{

    public class GetProductsHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetProductsQuery, Result<PagedList<ProductDto>>>
    {
        public async Task<Result<PagedList<ProductDto>>> Handle(GetProductsQuery request, CancellationToken ct)
        {
            var query = unitOfWork.Repository<Product>().Query()
                .AsNoTracking()
                .Include(p => p.Images)
                .Where(p => p.IsApproved && !p.IsDeleted);

            if (!string.IsNullOrEmpty(request.Params.Search))
                query = query.Where(p => p.Name.ToLower().Contains(request.Params.Search.ToLower()));

            if (request.Params.MinPrice.HasValue)
                query = query.Where(p => p.Price >= request.Params.MinPrice);

            query = request.Params.Sort switch
            {
                "priceAsc" => query.OrderBy(p => p.Price),
                "priceDesc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var totalItems = await query.CountAsync(ct);
            var items = await query.Skip((request.Params.PageIndex - 1) * request.Params.PageSize)
                                   .Take(request.Params.PageSize)
                                   .Select(p => new ProductDto
                                   {
                                       Id = p.Id,
                                       Name = p.Name,
                                       Price = p.Price,
                                       ImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary) != null
                                                ? p.Images.FirstOrDefault(i => i.IsPrimary)!.ImageUrl
                                                : p.Images.FirstOrDefault() != null ? p.Images.FirstOrDefault()!.ImageUrl
                                                : null
                                   })
                                                    .ToListAsync(ct);

            return Result<PagedList<ProductDto>>.Success(new PagedList<ProductDto>(items, totalItems));
        }
    }
}