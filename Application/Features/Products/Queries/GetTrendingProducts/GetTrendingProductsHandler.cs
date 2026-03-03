using Application.DTOs.Product;
using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetTrendingProducts
{
    public class GetTrendingProductsHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetTrendingProductsQuery, Result<List<ProductDto>>>
    {
        public async Task<Result<List<ProductDto>>> Handle(GetTrendingProductsQuery request, CancellationToken ct)
        {
            // Simple logic: Most sold or most recently approved with stock
            var products = await unitOfWork.Repository<Product>().Query()
                .AsNoTracking()
                .Include(p => p.Images)
                .Where(p => p.IsApproved && p.StockQuantity > 0)
                .OrderByDescending(p => p.CreatedAt) // Or p.SalesCount
                .Take(8)
                .Select(p => new ProductDto { /* ... mapping ... */ })
                .ToListAsync(ct);

            return Result<List<ProductDto>>.Success(products);
        }
    }
}