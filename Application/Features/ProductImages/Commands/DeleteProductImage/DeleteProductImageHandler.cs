using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductImages.Commands.DeleteProductImage
{
    public class DeleteProductImageHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<DeleteProductImageCommand, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(DeleteProductImageCommand command, CancellationToken cancellationToken)
        {
            var vendorId = currentUserService.GetCurrentVendorId();
            if (vendorId == null || vendorId == Guid.Empty)
                return Result<Unit>.Failure("Unauthorized");

            var productImageRepo = unitOfWork.Repository<ProductImage>();

            var image = await productImageRepo.Query()
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == command.ImageId &&
                                         i.ProductId == command.ProductId &&
                                         i.Product.VendorId == vendorId, cancellationToken);

            if (image == null) return Result<Unit>.Failure("Image not found.");

            bool wasPrimary = image.IsPrimary;

            productImageRepo.Delete(image);

            if (wasPrimary)
            {
                var nextImage = await productImageRepo.Query()
                    .Where(i => i.ProductId == command.ProductId && !i.IsDeleted && i.Id != command.ImageId)
                    .OrderBy(i => i.SortOrder)
                    .FirstOrDefaultAsync(cancellationToken);

                nextImage?.IsPrimary = true;
            }

            await unitOfWork.Complete();
            return Result<Unit>.Success(Unit.Value);
        }
    }
}