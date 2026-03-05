using Application.DTOs.Category;
using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Categories.Queries.GetCategoryTree
{
    public class GetCategoryTreeHandler(IUnitOfWork unitOfWork)
     : IRequestHandler<GetCategoryTreeQuery, Result<List<CategoryDto>>>
    {
        public async Task<Result<List<CategoryDto>>> Handle(GetCategoryTreeQuery request, CancellationToken ct)
        {
            var allCategories = await unitOfWork.Repository<Category>()
                .Query()
                .AsNoTracking()
                .Include(x => x.ParentCategory)
                .ToListAsync(ct);

            var lookup = allCategories.ToLookup(c => c.ParentCategoryId);

            List<CategoryDto> BuildTree(Guid? parentId)
            {
                return lookup[parentId]
                    .Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Children = BuildTree(c.Id)
                    }).ToList();
            }

            var tree = BuildTree(null);
            return Result<List<CategoryDto>>.Success(tree);
        }
    }
}