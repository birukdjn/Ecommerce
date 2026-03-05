using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductImages.Commands.BulkAddProductImage
{
    public class BulkAddProductImagesHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<BulkAddProductImagesCommand, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(BulkAddProductImagesCommand command, CancellationToken cancellationToken)
        {
            var vendorId = currentUserService.GetCurrentVendorId();
            if (vendorId == null || vendorId == Guid.Empty)
                return Result<Unit>.Failure("Unauthorized");

            var productRepo = unitOfWork.Repository<Product>();
            var productImageRepo = unitOfWork.Repository<ProductImage>();

            var product = await productRepo.Query()
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == command.ProductId && p.VendorId == vendorId, cancellationToken);

            if (product == null)
                return Result<Unit>.Failure("Product not found or you do not have permission.");

            bool hasPrimary = product.Images.Any(pi => pi.IsPrimary);
            int maxSortOrder = product.Images.Any() ? product.Images.Max(pi => pi.SortOrder) : 0;

            foreach (var (image, index) in command.Images.Select((value, idx) => (value, idx)))
            {
                var newImage = new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = image.Url,
                    AltText = image.AltText,
                    IsPrimary = !hasPrimary && index == 0,
                    SortOrder = maxSortOrder + index + 1
                };
                productImageRepo.Add(newImage);
            }

            await unitOfWork.Complete();
            return Result<Unit>.Success(Unit.Value);
        }
    }
}