using Application.DTOs.Product;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetTrendingProducts
{

    public class GetTrendingProductsHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetTrendingProductsQuery, Result<List<ProductDto>>>
    {
        public async Task<Result<List<ProductDto>>> Handle(GetTrendingProductsQuery request, CancellationToken ct)
        {
            var products = await unitOfWork.Repository<Product>().Query()
                .AsNoTracking()
                .Include(p => p.Images)
                .Include(p => p.Vendor)
                .Where(p => p.Status == ProductStatus.Approved && p.StockQuantity > 0)
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Vendor = p.Vendor != null ? p.Vendor.StoreName : "Unknown Store",
                    ImageUrl = p.Images
                        .Where(i => i.IsPrimary)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault() ?? (p.Images.Any() ? p.Images.First().ImageUrl : null)
                })
                .ToListAsync(ct);

            return Result<List<ProductDto>>.Success(products);
        }
    }
}