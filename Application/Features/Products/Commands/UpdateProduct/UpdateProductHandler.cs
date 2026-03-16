using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<UpdateProductCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
        {
            if (!currentUserService.IsVendor())
                return Result<bool>.Failure("Unauthorized");
            var VendorId = currentUserService.GetCurrentVendorId();

            var productRepo = unitOfWork.Repository<Product>();

            var product = await productRepo.Query()
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Id == command.Id && VendorId == p.VendorId, cancellationToken);

            if (product == null) return Result<bool>.Failure("Product not found.");
            if (product.VendorId != currentUserService.GetCurrentVendorId())
                return Result<bool>.Failure("Unauthorized.");

            product.Name = command.Name;
            product.Price = command.Price;
            product.Description = command.Description;
            product.StockQuantity = command.StockQuantity;

            product.ProductCategories.Clear();
            foreach (var catId in command.CategoryIds)
                product.ProductCategories.Add(new ProductCategory { ProductId = product.Id, CategoryId = catId });

            unitOfWork.Repository<Product>().Update(product);
            return await unitOfWork.Complete() > 0 ? Result<bool>.Success(true) : Result<bool>.Failure("No changes applied.");
        }
    }
}