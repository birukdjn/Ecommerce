using Application.DTOs.Product;
using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductImages.Queries.GetTrashedImages
{
    public class GetTrashedImagesHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<GetTrashedImagesQuery, Result<List<ProductImageDto>>>
    {
        public async Task<Result<List<ProductImageDto>>> Handle(GetTrashedImagesQuery request, CancellationToken ct)
        {
            var vendorId = currentUserService.GetCurrentVendorId();

            // Use IgnoreQueryFilters to see the 'IsDeleted = true' records
            var trashedImages = await unitOfWork.Repository<ProductImage>().Query()
                .IgnoreQueryFilters()
                .Include(i => i.Product)
                .Where(i => i.ProductId == request.ProductId &&
                            i.Product.VendorId == vendorId &&
                            i.IsDeleted == true)
                .Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    AltText = i.AltText,
                    IsPrimary = i.IsPrimary
                })
                .ToListAsync(ct);

            return Result<List<ProductImageDto>>.Success(trashedImages);
        }
    }
}