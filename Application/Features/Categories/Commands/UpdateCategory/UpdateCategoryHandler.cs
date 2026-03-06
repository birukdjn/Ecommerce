using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Categories.Commands.UpdateCategory
{
    public class UpdateCategoryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
        : IRequestHandler<UpdateCategoryCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
        {
            if (!currentUserService.IsAdmin())
                return Result<bool>.Failure("Unauthorized");

            var categoryrepo = unitOfWork.Repository<Category>();

            if (command.ParentCategoryId == command.Id)
                return Result<bool>.Failure("A category cannot be its own parent.");

            var category = await categoryrepo.GetByIdAsync(command.Id);
            if (category == null)
                return Result<bool>.Failure("Category not found.");

            // Validate commission range
            if (command.CommissionPercentage < 0 || command.CommissionPercentage > 100)
                return Result<bool>.Failure("Commission percentage must be between 0 and 100.");

            // Circular reference protection
            if (command.ParentCategoryId.HasValue)
            {
                var isCircular = await IsDescendantAsync(
                    category.Id,
                    command.ParentCategoryId,
                    categoryrepo,
                    cancellationToken);

                if (isCircular)
                    return Result<bool>.Failure("Circular reference detected.");
            }

            category.Name = command.Name.Trim();
            category.Description = command.Description?.Trim();
            category.ParentCategoryId = command.ParentCategoryId;
            category.CommissionPercentage = command.CommissionPercentage;

            categoryrepo.Update(category);

            var result = await unitOfWork.Complete();

            return result > 0
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("No changes were applied.");
        }

        private async Task<bool> IsDescendantAsync(
            Guid categoryId,
            Guid? potentialParentId,
            IGenericRepository<Category> categoryRepo,
            CancellationToken cancellationToken)
        {
            var currentParentId = potentialParentId;

            while (currentParentId != null)
            {
                if (currentParentId == categoryId)
                    return true;

                var parent = await categoryRepo.GetByIdAsync(currentParentId.Value);
                if (parent == null)
                    break;

                currentParentId = parent.ParentCategoryId;
            }

            return false;
        }
    }
}