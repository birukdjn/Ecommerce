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
        public async Task<Result<List<CategoryDto>>> Handle(GetCategoryTreeQuery request, CancellationToken cancellationToken)
        {
            var categoryRepo = unitOfWork.Repository<Category>();
            var allCategories = await categoryRepo
                .Query()
                .AsNoTracking()
                .Include(x => x.ParentCategory)
                .ToListAsync(cancellationToken);

            var lookup = allCategories.ToLookup(c => c.ParentCategoryId);

            List<CategoryDto> BuildTree(Guid? parentId)
            {
                return [.. lookup[parentId]
                    .Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Children = BuildTree(c.Id)
                    })];
            }

            var tree = BuildTree(null);
            return Result<List<CategoryDto>>.Success(tree);
        }
    }
}