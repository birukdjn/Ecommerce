using Application.DTOs.Product;
using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetMyProducts
{
    public class GetMyProductsHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
  : IRequestHandler<GetMyProductsQuery, Result<PagedList<MyProductSummaryDto>>>
    {
        public async Task<Result<PagedList<MyProductSummaryDto>>> Handle(GetMyProductsQuery request, CancellationToken ct)
        {
            var vendorId = currentUserService.GetCurrentVendorId();

            // 1. Start with a clean query
            var query = unitOfWork.Repository<Product>().Query()
                .AsNoTracking()
                .Include(p => p.Images)
                .Where(p => p.VendorId == vendorId);

            if (!string.IsNullOrWhiteSpace(request.Params.Search))
            {
                var search = request.Params.Search.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(search)
                                      || (p.Description != null && p.Description.ToLower().Contains(search)));
            }

            query = request.Params.Sort switch
            {
                "priceAsc" => query.OrderBy(p => p.Price),
                "priceDesc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var totalItems = await query.CountAsync(ct);

            var pageSize = request.Params.PageSize > 0 ? request.Params.PageSize : 10;
            var pageIndex = request.Params.PageIndex > 0 ? request.Params.PageIndex : 1;

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new MyProductSummaryDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    ImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary) != null
                               ? p.Images.FirstOrDefault(i => i.IsPrimary)!.ImageUrl
                               : null
                })
                .ToListAsync(ct);

            return Result<PagedList<MyProductSummaryDto>>.Success(new PagedList<MyProductSummaryDto>(items, totalItems));
        }
    }
}