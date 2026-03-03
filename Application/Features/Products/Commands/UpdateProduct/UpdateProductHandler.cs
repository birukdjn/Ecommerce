using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<UpdateProductCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateProductCommand request, CancellationToken ct)
        {
            var product = await unitOfWork.Repository<Product>()
                .Query()
                .Include(p => p.Images)
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

            if (product == null) return Result<bool>.Failure("Product not found.");
            if (product.VendorId != currentUserService.GetCurrentVendorId())
                return Result<bool>.Failure("Unauthorized.");

            product.Name = request.Name;
            product.Price = request.Price;
            product.Description = request.Description;
            product.StockQuantity = request.StockQuantity;

            product.ProductCategories.Clear();
            foreach (var catId in request.CategoryIds)
                product.ProductCategories.Add(new ProductCategory { ProductId = product.Id, CategoryId = catId });

            product.Images.Clear();
            for (int i = 0; i < request.ImageUrls.Count; i++)
                product.Images.Add(new ProductImage { ImageUrl = request.ImageUrls[i], IsPrimary = i == 0 });

            unitOfWork.Repository<Product>().Update(product);
            return await unitOfWork.Complete() > 0 ? Result<bool>.Success(true) : Result<bool>.Failure("No changes applied.");
        }
    }
}