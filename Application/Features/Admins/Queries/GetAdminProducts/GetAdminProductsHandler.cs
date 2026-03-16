using Application.DTOs.Admin;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admins.Queries.GetAdminProducts
{
    public class GetAdminProductsHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetAdminProductsQuery, Result<PagedList<AdminProductDto>>>
    {
        public async Task<Result<PagedList<AdminProductDto>>> Handle(GetAdminProductsQuery request, CancellationToken cancellationToken)
        {
            if (!currentUserService.IsAdmin())
                return Result<PagedList<AdminProductDto>>.Failure("Unauthorized");

            var query = unitOfWork.Repository<Product>().Query()
                .IgnoreQueryFilters()
                .Include(p => p.Vendor)
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .AsNoTracking();

            if (request.Params.ProductStatus.HasValue)
                query = query.Where(p => p.Status == request.Params.ProductStatus.Value);


            if (request.Params.IsDeleted.HasValue)
                query = query.Where(p => p.IsDeleted == request.Params.IsDeleted.Value);

            if (request.Params.VendorId.HasValue)
                query = query.Where(p => p.VendorId == request.Params.VendorId.Value);

            if (request.Params.CategoryId.HasValue)
            {
                query = query.Where(p => p.ProductCategories
                    .Any(pc => pc.CategoryId == request.Params.CategoryId.Value));
            }

            var totalItems = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((request.Params.PageIndex - 1) * request.Params.PageSize)
                .Take(request.Params.PageSize)
                .Select(p => new AdminProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    VendorStoreName = p.Vendor.StoreName,
                    Price = p.Price,
                    Status = p.Status,
                    IsDeleted = p.IsDeleted,
                    CreatedAt = p.CreatedAt,
                    Categories = p.ProductCategories
                .Select(pc => pc.Category.Name)
                .ToList()
                })
                .ToListAsync(cancellationToken);

            return Result<PagedList<AdminProductDto>>.Success(new PagedList<AdminProductDto>(items, totalItems));
        }
    }
}