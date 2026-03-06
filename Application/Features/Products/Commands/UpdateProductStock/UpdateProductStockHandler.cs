using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands.UpdateProductStock
{
    public class UpdateProductStockHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<UpdateProductStockCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateProductStockCommand command, CancellationToken cancellationToken)
        {
            if (!currentUserService.IsVendor())
                return Result<bool>.Failure("Unauthorized");

            var vendorId = currentUserService.GetCurrentVendorId();

            var ProductRepo = unitOfWork.Repository<Product>();
            var product = await ProductRepo.Query()
            .FirstOrDefaultAsync(p => p.Id == command.Id && p.VendorId == vendorId);

            if (product == null) return Result<bool>.Failure("Product not found.");

            product.StockQuantity = command.NewQuantity;

            unitOfWork.Repository<Product>().Update(product);
            return await unitOfWork.Complete() > 0 ? Result<bool>.Success(true) : Result<bool>.Failure("Update failed.");
        }
    }
}