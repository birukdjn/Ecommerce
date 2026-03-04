using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductImages.Commands.UpdateProductImage
{
    public class UpdateProductImageHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<UpdateProductImageCommand, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(UpdateProductImageCommand command, CancellationToken ct)
        {
            var vendorId = currentUserService.GetCurrentVendorId();

            var image = await unitOfWork.Repository<ProductImage>().Query()
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == command.ImageId &&
                                          i.Product.VendorId == vendorId &&
                                          !i.IsDeleted, ct);

            if (image == null)
                return Result<Unit>.Failure("Image not found or has been deleted.");

            image.ImageUrl = command.NewUrl;
            image.AltText = command.AltText;

            unitOfWork.Repository<ProductImage>().Update(image);
            await unitOfWork.Complete();

            return Result<Unit>.Success(Unit.Value);
        }
    }
}