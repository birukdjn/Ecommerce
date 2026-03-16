using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands.DeleteProduct
{
    public class DeleteProductHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<DeleteProductCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
        {
            if (!currentUserService.IsVendor())
                return Result<bool>.Failure("Unauthorized");

            var vendorId = currentUserService.GetCurrentVendorId();

            var productRepo = unitOfWork.Repository<Product>();

            var product = await productRepo.Query()
                .FirstOrDefaultAsync(p => p.Id == command.Id && p.VendorId == vendorId);

            if (product == null)
                return Result<bool>.Failure("Product not found.");

            unitOfWork.Repository<Product>().Delete(product);
            return await unitOfWork.Complete() > 0
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("Delete failed.");
        }
    }
}