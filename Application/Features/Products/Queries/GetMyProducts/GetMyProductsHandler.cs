using Application.DTOs.Product;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetMyProducts
{
    public class GetMyProductsHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
  : IRequestHandler<GetMyProductsQuery, Result<PagedList<MyProductSummaryDto>>>
    {
        public async Task<Result<PagedList<MyProductSummaryDto>>> Handle(GetMyProductsQuery request, CancellationToken cancellationToken)
        {
            if (!currentUserService.IsVendor())
                return Result<PagedList<MyProductSummaryDto>>.Failure("Unauthorized");
            var vendorId = currentUserService.GetCurrentVendorId();

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

            var totalItems = await query.CountAsync(cancellationToken);

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
                    Status = p.Status,
                    StockQuantity = p.StockQuantity,
                    ImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary) != null
                               ? p.Images.FirstOrDefault(i => i.IsPrimary)!.ImageUrl
                               : null
                })
                .ToListAsync(cancellationToken);

            return Result<PagedList<MyProductSummaryDto>>.Success(new PagedList<MyProductSummaryDto>(items, totalItems));
        }
    }
}