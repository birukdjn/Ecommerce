using Application.DTOs;
using Domain.Common;
using Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Categories.Queries.GetCategories
{
    public class GetCategoriesHandler(IApplicationDbContext context)
        : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
    {
        public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken ct)
        {
            var categories = await context.Categories
                .Select(c => new CategoryDto(c.Id, c.Name, c.Description))
                .ToListAsync(ct);

            return Result<List<CategoryDto>>.Success(categories);
        }
    }
}