using Application.DTOs.Product;
using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetProductsByCategory
{
    public class GetProductsByCategoryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetProductsByCategoryQuery, Result<PagedList<ProductDto>>>
    {
        public async Task<Result<PagedList<ProductDto>>> Handle(GetProductsByCategoryQuery request, CancellationToken ct)
        {
            var allCategories = await unitOfWork.Repository<Category>().ListAllAsync();

            var categoryIds = new List<Guid>();
            var lookup = allCategories.ToLookup(c => c.ParentCategoryId);
            AccumulateCategoryIds(request.CategoryId, lookup, categoryIds);

            var query = unitOfWork.Repository<Product>().Query()
                .AsNoTracking()
                .Include(p => p.Images)
                .Include(p => p.Vendor)
                .Where(p => p.IsApproved && !p.IsDeleted)
                .Where(p => p.ProductCategories.Any(pc => categoryIds.Contains(pc.CategoryId)));

            if (!string.IsNullOrWhiteSpace(request.Params.Search))
            {
                var search = request.Params.Search.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(search));
            }

            query = request.Params.Sort switch
            {
                "priceAsc" => query.OrderBy(p => p.Price),
                "priceDesc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.StockQuantity)
            };

            var totalItems = await query.CountAsync(ct);

            var pageSize = request.Params.PageSize > 0 ? request.Params.PageSize : 10;
            var pageIndex = request.Params.PageIndex > 0 ? request.Params.PageIndex : 1;

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.Images
                        .Where(i => i.IsPrimary)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault(),
                    VendorName = p.Vendor != null ? p.Vendor.StoreName : "Unknown Vendor"
                })
                .ToListAsync(ct);

            return Result<PagedList<ProductDto>>.Success(new PagedList<ProductDto>(items, totalItems));
        }

        private void AccumulateCategoryIds(Guid parentId, ILookup<Guid?, Category> lookup, List<Guid> result)
        {
            result.Add(parentId);
            foreach (var child in lookup[parentId])
            {
                AccumulateCategoryIds(child.Id, lookup, result);
            }
        }
    }
}