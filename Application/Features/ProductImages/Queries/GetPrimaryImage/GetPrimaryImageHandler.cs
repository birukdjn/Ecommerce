using Application.DTOs.Product;
using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductImages.Queries.GetPrimaryImage
{
    public class GetPrimaryImageHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetPrimaryImageQuery, Result<ProductImageDto>>
    {
        public async Task<Result<ProductImageDto>> Handle(GetPrimaryImageQuery request, CancellationToken ct)
        {
            var primaryImage = await unitOfWork.Repository<ProductImage>().Query()
                .AsNoTracking()
                .Where(i => i.ProductId == request.ProductId && i.IsPrimary)
                .Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    AltText = i.AltText,
                    IsPrimary = true

                })
                .FirstOrDefaultAsync(ct);

            if (primaryImage == null)
                return Result<ProductImageDto>.Failure("No primary image found for this product.");

            return Result<ProductImageDto>.Success(primaryImage);
        }
    }
}