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
                .Where(p => p.Status == ProductStatus.Approved && p.StockQuantity > 0)
                .Select(p => new
                {
                    Product = p,
                    AvgRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
                    ReviewCount = p.Reviews.Count()
                })
                .OrderByDescending(p => p.AvgRating)
                .ThenByDescending(p => p.ReviewCount)
                .ThenByDescending(p => p.Product.CreatedAt)
                .Take(10)
                .Select(p => new ProductDto
                {
                    Id = p.Product.Id,
                    Name = p.Product.Name,
                    Price = p.Product.Price,
                    Vendor = p.Product.Vendor != null
                        ? p.Product.Vendor.StoreName
                        : "Unknown Store",
                    ImageUrl = p.Product.Images
                        .Where(i => i.IsPrimary)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault()
                        ?? p.Product.Images.Select(i => i.ImageUrl).FirstOrDefault()
                })
                .ToListAsync(ct);

            return Result<List<ProductDto>>.Success(products);
        }
    }
}