using Application.DTOs.Product;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetProducts
{

    public class GetProductsHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetProductsQuery, Result<PagedList<ProductDto>>>
    {
        public async Task<Result<PagedList<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var query = unitOfWork.Repository<Product>().Query()
                .AsNoTracking()
                .Include(p => p.Images)
                .Where(p => p.Status == ProductStatus.Approved);

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

            var totalItems = await query.CountAsync(cancellationToken);
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
                                                    .ToListAsync(cancellationToken);

            return Result<PagedList<ProductDto>>.Success(new PagedList<ProductDto>(items, totalItems));
        }
    }
}