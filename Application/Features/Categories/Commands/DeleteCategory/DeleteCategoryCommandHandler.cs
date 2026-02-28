using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Categories.Commands.DeleteCategory
{
    public class DeleteCategoryCommandHandler(
        IUnitOfWork unitOfWork,
        IApplicationDbContext context)
        : IRequestHandler<DeleteCategoryCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var categoryRepo = unitOfWork.Repository<Category>();

            var category = await categoryRepo.GetByIdAsync(request.Id);
            if (category == null || category.IsDeleted)
                return Result<bool>.Failure("Category not found.");

            var hasProducts = await context.ProductCategories
                .AnyAsync(pc => pc.CategoryId == request.Id && !pc.Product.IsDeleted, cancellationToken);

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