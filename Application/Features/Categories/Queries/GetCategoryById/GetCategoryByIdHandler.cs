using Application.DTOs;
using Domain.Common;
using Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Categories.Queries.GetCategoryById
{
    public class GetCategoryByIdHandler(IApplicationDbContext context)
        : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
    {
        public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken ct)
        {
            var category = await context.Categories
                .Where(c => c.Id == request.Id)
                .Select(c => new CategoryDto(c.Id, c.Name, c.Description))
                .FirstOrDefaultAsync(ct);

            if (category == null)
                return Result<CategoryDto>.Failure("Category not found.");

            return Result<CategoryDto>.Success(category);
        }
    }
}