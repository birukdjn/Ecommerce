using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProductStock
{
    public class UpdateProductStockHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<UpdateProductStockCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateProductStockCommand request, CancellationToken ct)
        {
            var product = await unitOfWork.Repository<Product>().GetByIdAsync(request.Id);

            if (product == null) return Result<bool>.Failure("Product not found.");
            if (product.VendorId != currentUserService.GetCurrentVendorId())
                return Result<bool>.Failure("Unauthorized.");

            product.StockQuantity = request.NewQuantity;

            unitOfWork.Repository<Product>().Update(product);
            return await unitOfWork.Complete() > 0 ? Result<bool>.Success(true) : Result<bool>.Failure("Update failed.");
        }
    }
}