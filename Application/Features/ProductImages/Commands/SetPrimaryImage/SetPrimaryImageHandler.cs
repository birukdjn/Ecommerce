using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductImages.Commands.SetPrimaryImage
{
    public class SetPrimaryImageHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<SetPrimaryImageCommand, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(SetPrimaryImageCommand request, CancellationToken ct)
        {
            var vendorId = currentUserService.GetCurrentVendorId();

            var product = await unitOfWork.Repository<Product>()
                .Query()
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.VendorId == vendorId, ct);

            if (product == null)
                return Result<Unit>.Failure("Product not found or you do not have permission.");

            var targetImage = product.Images.FirstOrDefault(i => i.Id == request.ImageId && !i.IsDeleted);
            if (targetImage == null)
                return Result<Unit>.Failure("Image not found or has been deleted.");

            foreach (var img in product.Images.Where(i => !i.IsDeleted))
            {
                img.IsPrimary = false;
            }

            targetImage.IsPrimary = true;

            await unitOfWork.Complete();
            return Result<Unit>.Success(Unit.Value);
        }
    }
}