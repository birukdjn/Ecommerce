using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<CreateProductCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
        {
            if (!currentUserService.IsVendor())
                return Result<Guid>.Failure("User is not registered as a vendor.");

            var vendorId = currentUserService.GetCurrentVendorId();
            if (vendorId == null || vendorId == Guid.Empty)
                return Result<Guid>.Failure("Vendor profile not found.");

            var vendorRepo = unitOfWork.Repository<Vendor>();
            var productRepo = unitOfWork.Repository<Product>();
            var productImageRepo = unitOfWork.Repository<ProductImage>();
            var vendor = await vendorRepo.GetByIdAsync(vendorId.Value);

            if (vendor == null || vendor.Status != VendorStatus.Approved)
                return Result<Guid>.Failure("Only approved vendors can create products.");

            var name = command.Name.Trim();
            var description = command.Description?.Trim();
            var price = command.Price;
            var stockQuantity = command.StockQuantity;

            var existingProduct = await productRepo.Query()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Name == name && vendorId == p.VendorId, cancellationToken);

            if (existingProduct != null)
            {
                existingProduct.IsDeleted = false;

                bool priceChanged = existingProduct.Price != price;
                bool descriptionChanged = existingProduct.Description != description;
                bool stockChanged = existingProduct.StockQuantity != stockQuantity;

                if (priceChanged)
                {
                    existingProduct.Price = price;
                    existingProduct.IsApproved = false;
                }

                if (descriptionChanged && !priceChanged)
                {
                    existingProduct.Description = description;
                }

                if (stockChanged && !priceChanged && !descriptionChanged)
                {
                    existingProduct.StockQuantity = stockQuantity;
                }

                var deletedImages = await productImageRepo.Query()
                    .IgnoreQueryFilters()
                    .Where(i => i.ProductId == existingProduct.Id && i.IsDeleted)
                    .ToListAsync(cancellationToken);

                foreach (var img in deletedImages)
                {
                    img.IsDeleted = false;
                }

                productRepo.Update(existingProduct);
                var updateResult = await unitOfWork.Complete();
                return updateResult > 0 ? Result<Guid>.Success(existingProduct.Id) : Result<Guid>.Failure("Failed to update existing product.");
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                Price = price,
                StockQuantity = stockQuantity,
                VendorId = vendor.Id,
                IsApproved = false,
                CreatedAt = DateTime.UtcNow
            };

            var rootCategoryIds = new HashSet<Guid>();
            foreach (var catId in command.CategoryIds)
            {
                var category = await unitOfWork.Repository<Category>()
                    .Query()
                    .Include(c => c.ChildCategories)
                    .Include(c => c.ParentCategory)
                    .FirstOrDefaultAsync(c => c.Id == catId, cancellationToken);

                if (category == null) return Result<Guid>.Failure($"Category {catId} not found.");
                if (category.ChildCategories.Any())
                    return Result<Guid>.Failure($"'{category.Name}' is a parent category. Please pick a specific sub-category.");

                var root = category;
                while (root.ParentCategoryId.HasValue)
                    root = await unitOfWork.Repository<Category>().GetByIdAsync(root.ParentCategoryId.Value);

                rootCategoryIds.Add(root.Id);
                if (rootCategoryIds.Count > 1)
                    return Result<Guid>.Failure("A product cannot belong to two different departments.");

                product.ProductCategories.Add(new ProductCategory
                {
                    ProductId = product.Id,
                    CategoryId = catId
                });
            }

            productRepo.Add(product);
            var result = await unitOfWork.Complete();
            return result > 0 ? Result<Guid>.Success(product.Id) : Result<Guid>.Failure("Failed to save product.");
        }
    }
}