using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductImages.Commands.ReorderProductImage
{
    public class ReorderProductImagesHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<ReorderProductImagesCommand, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(ReorderProductImagesCommand request, CancellationToken ct)
        {
            var vendorId = currentUserService.GetCurrentVendorId();

            // Load product and images
            var product = await unitOfWork.Repository<Product>()
                .Query()
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.VendorId == vendorId, ct);

            if (product == null)
                return Result<Unit>.Failure("Product not found or you do not have permission.");

            var images = product.Images.Where(i => !i.IsDeleted).ToList();

            var imageIds = images.Select(i => i.Id).ToHashSet();
            if (!request.OrderedIds.All(id => imageIds.Contains(id)))
                return Result<Unit>.Failure("One or more images do not belong to this product.");

            for (int i = 0; i < request.OrderedIds.Count; i++)
            {
                var image = images.First(x => x.Id == request.OrderedIds[i]);
                image.SortOrder = i + 1;
            }

            await unitOfWork.Complete();
            return Result<Unit>.Success(Unit.Value);
        }
    }
}