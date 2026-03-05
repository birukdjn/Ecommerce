using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductImages.Commands.RestoreProductImage
{
    public class RestoreProductImageHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<RestoreProductImageCommand, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(RestoreProductImageCommand command, CancellationToken cancellationToken)
        {
            var vendorId = currentUserService.GetCurrentVendorId();
            if (vendorId == null || vendorId == Guid.Empty)
                return Result<Unit>.Failure("Unauthorized");

            var productImageRepo = unitOfWork.Repository<ProductImage>();

            var image = await productImageRepo.Query()
                .IgnoreQueryFilters()
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == command.ImageId &&
                                          i.ProductId == command.ProductId &&
                                          i.Product.VendorId == vendorId, cancellationToken);

            if (image == null || !image.IsDeleted)
                return Result<Unit>.Failure("Deleted image not found.");

            image.IsDeleted = false;
            image.LastModifiedAt = DateTime.UtcNow;

            var activePrimaryExists = await productImageRepo.Query()
                .Where(i => i.ProductId == command.ProductId && !i.IsDeleted)
                .AnyAsync(i => i.IsPrimary, cancellationToken);

            if (!activePrimaryExists)
                image.IsPrimary = true;

            productImageRepo.Update(image);
            await unitOfWork.Complete();

            return Result<Unit>.Success(Unit.Value);
        }
    }
}