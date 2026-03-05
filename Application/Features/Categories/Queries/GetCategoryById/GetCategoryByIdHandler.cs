using Application.DTOs.Category;
using Domain.Common;
using Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Categories.Queries.GetCategoryById
{
    public class GetCategoryByIdHandler(IApplicationDbContext context)
        : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
    {
        public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var category = await context.Categories
                .Where(c => c.Id == request.Id)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                }).FirstOrDefaultAsync(cancellationToken);


            if (category == null)
                return Result<CategoryDto>.Failure("Category not found.");

            return Result<CategoryDto>.Success(category);
        }
    }
}