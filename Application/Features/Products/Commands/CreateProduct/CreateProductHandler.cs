using Application.Interfaces;
using Application.Templates.Email;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IJobService jobService) : IRequestHandler<CreateProductCommand, Result<Guid>>
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
            var vendor = await vendorRepo.Query()
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == vendorId.Value, cancellationToken);

            if (vendor == null || vendor.Status != VendorStatus.Active)
                return Result<Guid>.Failure("Only approved vendors can create products.");

            var name = command.Name.Trim().ToLower();
            var description = command.Description?.Trim();
            var price = command.Price;
            var stockQuantity = command.StockQuantity;

            var existingProduct = await productRepo.Query()
                .FirstOrDefaultAsync(p => p.Name.ToLower() == name && vendorId == p.VendorId, cancellationToken);

            if (existingProduct != null)
            {
                return Result<Guid>.Failure("Product Already Added");

            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                Price = price,
                StockQuantity = stockQuantity,
                VendorId = vendor.Id,
                Status = ProductStatus.Pending,
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
                {
                    var parent = await unitOfWork.Repository<Category>().GetByIdAsync(root.ParentCategoryId.Value);
                    if (parent == null) break;
                    root = parent;
                }

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

            if (result > 0)
            {
                // Send Background Email to Vendor
                if (vendor.User != null && !string.IsNullOrEmpty(vendor.User.Email))
                {
                    // We use the raw name from the command for the email to keep the original casing
                    var displayProductName = command.Name.Trim();

                    jobService.Enqueue<IEmailSender>(sender =>
                        sender.SendEmailAsync(
                            vendor.User.Email,
                            $"Product Submitted: {displayProductName}",
                            EmailTemplates.GetProductSubmittedEmail(vendor.User.FullName ?? vendor.StoreName, displayProductName)
                        ));
                }

                return Result<Guid>.Success(product.Id);
            }

            return Result<Guid>.Failure("Failed to save product.");
        }
    }
}