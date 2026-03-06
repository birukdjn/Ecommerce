using Application.DTOs.Product;
using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetProductById
{
    public class GetProductByIdHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
    {
        public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await unitOfWork.Repository<Product>().Query()
                .AsNoTracking()
                .Include(p => p.Images)
                .Include(p => p.Vendor)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (product == null)
                return Result<ProductDto>.Failure("Product not found.");

            var dto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                ImageUrl = product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                           ?? product.Images.FirstOrDefault()?.ImageUrl,
                VendorName = product.Vendor?.StoreName ?? "Unknown Vendor"
            };

            return Result<ProductDto>.Success(dto);
        }
    }
}