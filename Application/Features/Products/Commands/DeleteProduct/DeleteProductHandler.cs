using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Products.Commands.DeleteProduct
{
    public class DeleteProductHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<DeleteProductCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken ct)
        {
            var product = await unitOfWork.Repository<Product>().GetByIdAsync(request.Id);
            if (product == null) return Result<bool>.Failure("Product not found.");

            if (product.VendorId != currentUserService.GetCurrentVendorId())
                return Result<bool>.Failure("You do not have permission to delete this product.");

            unitOfWork.Repository<Product>().Delete(product);
            return await unitOfWork.Complete() > 0
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("Delete failed.");
        }
    }
}