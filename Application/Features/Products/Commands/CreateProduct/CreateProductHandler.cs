using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<CreateProductCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken ct)
        {
            if (!currentUserService.IsVendor())
                return Result<Guid>.Failure("User is not registered as a vendor.");

            var vendorId = currentUserService.GetCurrentVendorId();
            if (vendorId == null)
                return Result<Guid>.Failure("Vendor profile not found.");

            var vendor = await unitOfWork.Repository<Vendor>().GetByIdAsync(vendorId.Value);

            if (vendor == null || vendor.Status != VendorStatus.Approved)
                return Result<Guid>.Failure("Only approved vendors can create products.");

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                VendorId = vendor.Id,
                IsApproved = false
            };

            foreach (var catId in request.CategoryIds)
            {
                product.ProductCategories.Add(new ProductCategory
                {
                    ProductId = product.Id,
                    CategoryId = catId
                });
            }

            for (int i = 0; i < request.ImageUrls.Count; i++)
            {
                product.Images.Add(new ProductImage
                {
                    ImageUrl = request.ImageUrls[i],
                    IsPrimary = i == 0
                });
            }

            unitOfWork.Repository<Product>().Add(product);

            var result = await unitOfWork.Complete();
            return result > 0 ? Result<Guid>.Success(product.Id) : Result<Guid>.Failure("Failed to save product.");
        }
    }
}