using Application.DTOs;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Categories.Queries.GetCategoryTree
{
    public class GetCategoryTreeHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetCategoryTreeQuery, List<CategoryDto>>
    {
        public async Task<List<CategoryDto>> Handle(GetCategoryTreeQuery request, CancellationToken ct)
        {

            var allCategories = await unitOfWork.Repository<Category>()
                .Query()
                .AsNoTracking()
                .Include(x => x.ParentCategory)
                .ToListAsync(ct);

            // 2. Build the lookup
            var lookup = allCategories.ToLookup(c => c.ParentCategoryId);

            // 3. Recursive function to build DTOs
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

            return BuildTree(null);
        }
    }
}