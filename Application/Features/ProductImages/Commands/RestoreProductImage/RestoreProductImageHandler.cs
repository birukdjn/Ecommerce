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
        public async Task<Result<Unit>> Handle(RestoreProductImageCommand request, CancellationToken ct)
        {
            var vendorId = currentUserService.GetCurrentVendorId();

            var image = await unitOfWork.Repository<ProductImage>().Query()
                .IgnoreQueryFilters()
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == request.ImageId &&
                                          i.ProductId == request.ProductId &&
                                          i.Product.VendorId == vendorId, ct);

            if (image == null || !image.IsDeleted)
                return Result<Unit>.Failure("Deleted image not found.");

            image.IsDeleted = false;
            image.LastModifiedAt = DateTime.UtcNow;

            var activePrimaryExists = await unitOfWork.Repository<ProductImage>().Query()
                .Where(i => i.ProductId == request.ProductId && !i.IsDeleted)
                .AnyAsync(i => i.IsPrimary, ct);

            if (!activePrimaryExists)
                image.IsPrimary = true;

            unitOfWork.Repository<ProductImage>().Update(image);
            await unitOfWork.Complete();

            return Result<Unit>.Success(Unit.Value);
        }
    }
}