using Application.DTOs.Product;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetMyProductDetail
{
    public class GetMyProductDetailHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<GetMyProductDetailQuery, Result<MyProductDetailDto>>
    {
        public async Task<Result<MyProductDetailDto>> Handle(GetMyProductDetailQuery request, CancellationToken cancellationToken)
        {
            if (!currentUserService.IsVendor())
                return Result<MyProductDetailDto>.Failure("Unauthorized");

            var vendorId = currentUserService.GetCurrentVendorId();

            var product = await unitOfWork.Repository<Product>().Query()
                .AsNoTracking()
                .Include(p => p.Images)
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == request.Id && p.VendorId == vendorId, cancellationToken);

            if (product == null)
                return Result<MyProductDetailDto>.Failure("Product not found");

            var dto = new MyProductDetailDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description ?? string.Empty,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Status = product.Status,
                CreatedAt = product.CreatedAt,
                ImageUrls = product.Images.Select(i => i.ImageUrl).ToList(),
                CategoryIds = product.ProductCategories.Select(pc => pc.CategoryId).ToList(),
                CategoryNames = product.ProductCategories.Select(pc => pc.Category.Name).ToList()
            };

            return Result<MyProductDetailDto>.Success(dto);
        }
    }
}