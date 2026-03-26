using Application.DTOs.Product;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetProductById
{
    public class GetProductByIdHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetProductByIdQuery, Result<ProductDetailDto>>
    {
        public async Task<Result<ProductDetailDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await unitOfWork.Repository<Product>().Query()
                .AsNoTracking()
                .Include(p => p.Images)
                .Include(p => p.ProductCategories)
                .ThenInclude(p => p.Category)
                .Include(p => p.Vendor)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (product == null)
                return Result<ProductDetailDto>.Failure("Product not found.");

            var dto = new ProductDetailDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description!,
                Price = product.Price,
                ImageUrl = product.Images.Select(i => i.ImageUrl).ToList(),
                Vendor = product.Vendor.StoreName,
                Categories = product.ProductCategories.Select(pc => pc.Category.Name).ToList()

            };

            return Result<ProductDetailDto>.Success(dto);
        }
    }
}