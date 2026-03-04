using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductImages.Commands.DeleteProductImage
{
    public class DeleteProductImageHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<DeleteProductImageCommand, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(DeleteProductImageCommand request, CancellationToken ct)
        {
            var vendorId = currentUserService.GetCurrentVendorId();

            var image = await unitOfWork.Repository<ProductImage>().Query()
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == request.ImageId &&
                                         i.ProductId == request.ProductId &&
                                         i.Product.VendorId == vendorId, ct);

            if (image == null) return Result<Unit>.Failure("Image not found.");

            bool wasPrimary = image.IsPrimary;

            unitOfWork.Repository<ProductImage>().Delete(image);

            if (wasPrimary)
            {
                var nextImage = await unitOfWork.Repository<ProductImage>().Query()
                    .Where(i => i.ProductId == request.ProductId && !i.IsDeleted && i.Id != request.ImageId)
                    .OrderBy(i => i.SortOrder)
                    .FirstOrDefaultAsync(ct);

                nextImage?.IsPrimary = true;
            }

            await unitOfWork.Complete();
            return Result<Unit>.Success(Unit.Value);
        }
    }
}