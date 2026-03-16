using Application.DTOs.Category;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Categories.Commands.CreateCategory
{
    public class CreateCategoryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
    {
        public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
        {
            if (currentUserService.IsAdmin())
                return Result<CategoryDto>.Failure("Unauthorized");

            var categoryRepo = unitOfWork.Repository<Category>();

            // Normalize input
            var name = command.Name.Trim();
            var parentCategoryId = command.ParentCategoryId;
            var commissionPercentage = command.CommissionPercentage;
            var description = command.Description?.Trim();

            // Check for existing category
            var existingCategory = await categoryRepo.Query()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c =>
                    c.Name.ToLower() == name.ToLower() &&
                    c.ParentCategoryId == parentCategoryId &&
                    c.CommissionPercentage == commissionPercentage,
                    cancellationToken);

            if (existingCategory != null)
            {
                if (!existingCategory.IsDeleted)
                    return Result<CategoryDto>.Failure("Category already exists");

                // Restore deleted category
                existingCategory.IsDeleted = false;
                existingCategory.Name = name;
                existingCategory.Description = description;
                existingCategory.CommissionPercentage = commissionPercentage;

                categoryRepo.Update(existingCategory);

                var restoredResult = await unitOfWork.Complete();
                return restoredResult > 0
                    ? Result<CategoryDto>.Success(new CategoryDto
                    {
                        Id = existingCategory.Id,
                        Name = existingCategory.Name,
                        Description = existingCategory.Description,

                    })
                    : Result<CategoryDto>.Failure("Failed to restore category");
            }

            // Validation
            if (commissionPercentage < 0 || commissionPercentage > 100)
                return Result<CategoryDto>.Failure("Commission percentage must be between 0 and 100.");

            if (parentCategoryId.HasValue)
            {
                var parentCategory = await categoryRepo.GetByIdAsync(parentCategoryId.Value);
                if (parentCategory == null)
                    return Result<CategoryDto>.Failure("Parent category not found.");

                if (commissionPercentage < parentCategory.CommissionPercentage)
                    return Result<CategoryDto>.Failure($"Category commission ({commissionPercentage}%) cannot be lower than its parent '{parentCategory.Name}' ({parentCategory.CommissionPercentage}%).");
            }

            // Create new category
            var category = new Category
            {
                Name = name,
                Description = description,
                ParentCategoryId = parentCategoryId,
                CommissionPercentage = commissionPercentage
            };

            categoryRepo.Add(category);
            var result = await unitOfWork.Complete();

            return result > 0
                ? Result<CategoryDto>.Success(new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,

                })
                : Result<CategoryDto>.Failure("Failed to create category");
        }
    }
}