using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductImages.Commands.ReorderProductImage
{
    public class ReorderProductImagesHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<ReorderProductImagesCommand, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(ReorderProductImagesCommand command, CancellationToken cancellationToken)
        {
            var vendorId = currentUserService.GetCurrentVendorId();
            if (vendorId == null || vendorId == Guid.Empty)
                return Result<Unit>.Failure("Unauthorized");

            var productRepo = unitOfWork.Repository<Product>();

            var product = await productRepo.Query()
                .Include(p => p.Images)
                .FirstOrDefaultAsync(
                    p => p.Id == command.ProductId && p.VendorId == vendorId,
                    cancellationToken);

            if (product == null)
                return Result<Unit>.Failure("Product not found or you do not have permission.");

            var images = product.Images
                .Where(i => !i.IsDeleted)
                .ToList();

            var imageIds = images.Select(i => i.Id).ToHashSet();

            // Validation
            if (command.OrderedIds.Count != images.Count)
                return Result<Unit>.Failure("All product images must be included in the reorder request.");

            if (command.OrderedIds.Distinct().Count() != command.OrderedIds.Count)
                return Result<Unit>.Failure("Duplicate image IDs detected.");

            if (!command.OrderedIds.All(id => imageIds.Contains(id)))
                return Result<Unit>.Failure("One or more images do not belong to this product.");

            var imageDictionary = images.ToDictionary(i => i.Id);

            for (int i = 0; i < command.OrderedIds.Count; i++)
            {
                imageDictionary[command.OrderedIds[i]].SortOrder = i + 1;
            }

            await unitOfWork.Complete();

            return Result<Unit>.Success(Unit.Value);
        }
    }
}