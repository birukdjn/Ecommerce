
using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductImages.Commands.AddProductImage
{
    public class AddProductImageHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<AddProductImageCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(AddProductImageCommand command, CancellationToken ct)
        {
            var vendorId = currentUserService.GetCurrentVendorId();

            var product = await unitOfWork.Repository<Product>()
                .Query()
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == command.ProductId && p.VendorId == vendorId, ct);

            if (product == null)
                return Result<Guid>.Failure("Product not found or you do not have permission.");

            var hasActiveImages = product.Images.Any(i => !i.IsDeleted);

            var maxSortOrder = product.Images.Any() ? product.Images.Max(pi => pi.SortOrder) : 0;

            var image = new ProductImage
            {
                ProductId = product.Id,
                ImageUrl = command.ImageUrl,
                AltText = command.AltText,
                IsPrimary = !hasActiveImages,
                SortOrder = maxSortOrder + 1
            };

            unitOfWork.Repository<ProductImage>().Add(image);
            await unitOfWork.Complete();

            return Result<Guid>.Success(image.Id);
        }
    }
}