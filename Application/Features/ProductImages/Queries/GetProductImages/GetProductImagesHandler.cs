using Application.DTOs.Product;
using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductImages.Queries.GetProductImages
{
    public class GetProductImagesHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetProductImagesQuery, Result<List<ProductImageDto>>>
    {
        public async Task<Result<List<ProductImageDto>>> Handle(GetProductImagesQuery request, CancellationToken ct)
        {
            var images = await unitOfWork.Repository<ProductImage>().Query()
                .Where(i => i.ProductId == request.ProductId)
                .OrderByDescending(i => i.IsPrimary)
                .Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary
                })
                .ToListAsync(ct);

            return Result<List<ProductImageDto>>.Success(images);
        }
    }
}