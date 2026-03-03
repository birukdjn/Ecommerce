using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Categories.Commands.CreateCategory
{
    public class CreateCategoryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<CreateCategoryCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken ct)
        {
            var categoryRepo = unitOfWork.Repository<Category>();

            if (request.CommissionPercentage < 0 || request.CommissionPercentage > 100)
                return Result<Guid>.Failure("Commission percentage must be between 0 and 100.");

            if (request.ParentCategoryId.HasValue)
            {
                var parentCategory = await categoryRepo.GetByIdAsync(request.ParentCategoryId.Value);

                if (parentCategory == null)
                    return Result<Guid>.Failure("Parent category not found.");

                if (request.CommissionPercentage < parentCategory.CommissionPercentage)
                    return Result<Guid>.Failure($"Category commission ({request.CommissionPercentage}%) cannot be lower than its parent '{parentCategory.Name}' ({parentCategory.CommissionPercentage}%).");
            }

            var category = new Category
            {
                Name = request.Name,
                Description = request.Description,
                ParentCategoryId = request.ParentCategoryId,
                CommissionPercentage = request.CommissionPercentage
            };

            categoryRepo.Add(category);
            var result = await unitOfWork.Complete();

            return result > 0
                ? Result<Guid>.Success(category.Id)
                : Result<Guid>.Failure("Failed to create category.");
        }
    }
}