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
            var category = await repo.GetByIdAsync(request.Id);

            if (category == null) return Result<bool>.Failure("Category not found.");

            category.Name = request.Name;
            category.Description = request.Description;

            repo.Update(category);
            var result = await unitOfWork.Complete();

            return result > 0
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("No changes were applied.");
        }
    }
}