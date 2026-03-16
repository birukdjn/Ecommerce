using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductImages.Commands.UpdateProductImage
{
    public class UpdateProductImageHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<UpdateProductImageCommand, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(UpdateProductImageCommand command, CancellationToken cancellationToken)
        {
            var vendorId = currentUserService.GetCurrentVendorId();
            if (vendorId == null || vendorId == Guid.Empty)
                return Result<Unit>.Failure("Unauthorized");

            var productImageRepo = unitOfWork.Repository<ProductImage>();

            var image = await productImageRepo.Query()
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == command.ImageId &&
                                          i.Product.VendorId == vendorId &&
                                          !i.IsDeleted, cancellationToken);

            if (image == null)
                return Result<Unit>.Failure("Image not found or has been deleted.");

            image.ImageUrl = command.NewUrl;
            image.AltText = command.AltText;

            productImageRepo.Update(image);
            await unitOfWork.Complete();

            return Result<Unit>.Success(Unit.Value);
        }
    }
}