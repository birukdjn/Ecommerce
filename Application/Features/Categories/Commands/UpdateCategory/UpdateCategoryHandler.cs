using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Categories.Commands.UpdateCategory
{
    public class UpdateCategoryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateCategoryCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateCategoryCommand request, CancellationToken ct)
        {
            var repo = unitOfWork.Repository<Category>();
            if (request.ParentCategoryId == request.Id)
                return Result<bool>.Failure("A category cannot be its own parent.");

            var category = await repo.GetByIdAsync(request.Id);

            if (category == null) return Result<bool>.Failure("Category not found.");

            if (request.ParentCategoryId.HasValue)
            {
                if (IsDescendant(category, request.ParentCategoryId.Value))
                    return Result<bool>.Failure("Circular reference: Cannot move a category under one of its own children.");
            }

            category.Name = request.Name;
            category.Description = request.Description;
            category.ParentCategoryId = request.ParentCategoryId;
            category.CommissionPercentage = request.CommissionPercentage;

            repo.Update(category);
            var result = await unitOfWork.Complete();

            return result > 0
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("No changes were applied.");
        }

        private bool IsDescendant(Category current, Guid targetParentId)
        {
            foreach (var child in current.ChildCategories)
            {
                if (child.Id == targetParentId) return true;
                if (IsDescendant(child, targetParentId)) return true;
            }
            return false;
        }
    }
}