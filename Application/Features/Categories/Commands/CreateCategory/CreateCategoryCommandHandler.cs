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

            var category = new Category 
            { 
                Name = request.Name, 
                Description = request.Description 
            };

            categoryRepo.Add(category);

            var result = await unitOfWork.Complete();

            if (result <= 0)
                return Result<Guid>.Failure("Failed to create category.");

            return Result<Guid>.Success(category.Id);
        }
    }
}