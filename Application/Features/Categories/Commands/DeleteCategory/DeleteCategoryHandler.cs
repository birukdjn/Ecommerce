using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Categories.Commands.DeleteCategory
{
    public class DeleteCategoryCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
        : IRequestHandler<DeleteCategoryCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
        {
            if (!currentUserService.IsAdmin())
                return Result<bool>.Failure("Unauthorized");

            var categoryRepo = unitOfWork.Repository<Category>();
            var productCategoryRepo = unitOfWork.Repository<ProductCategory>();
            var productRepo = unitOfWork.Repository<Product>();

            var category = await categoryRepo.GetByIdAsync(command.Id);
            if (category == null || category.IsDeleted)
                return Result<bool>.Failure("Category not found.");

            // Check for active subcategories
            var hasChildren = await categoryRepo.Query()
                .AnyAsync(c =>
                    c.ParentCategoryId == command.Id,
                    cancellationToken);

            if (hasChildren)
                return Result<bool>.Failure("Cannot delete category because it has subcategories.");

            // Check for active products in this category
            var hasProducts = await productCategoryRepo.Query()
                .Where(pc => pc.CategoryId == command.Id)
                .Join(
                    productRepo.Query(),
                    pc => pc.ProductId,
                    p => p.Id,
                    (pc, p) => p
                )
                .AnyAsync(p => !p.IsDeleted, cancellationToken);

            if (hasProducts)
                return Result<bool>.Failure("Cannot delete category because it contains active products.");

            categoryRepo.Delete(category);

            var result = await unitOfWork.Complete();

            return result > 0
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("Failed to delete the category.");
        }
    }
}