using Application.DTOs.Category;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Categories.Queries.GetCategoryTree
{
    public class GetCategoryTreeHandler(IUnitOfWork unitOfWork)
     : IRequestHandler<GetCategoryTreeQuery, Result<List<CategoryTreeDto>>>
    {
        public async Task<Result<List<CategoryTreeDto>>> Handle(GetCategoryTreeQuery request, CancellationToken cancellationToken)
        {
            var categoryRepo = unitOfWork.Repository<Category>();
            var allCategories = await categoryRepo
                .Query()
                .AsNoTracking()
                .Include(x => x.ParentCategory)
                .ToListAsync(cancellationToken);

            var lookup = allCategories.ToLookup(c => c.ParentCategoryId);

            List<CategoryTreeDto> BuildTree(Guid? parentId)
            {
                return [.. lookup[parentId]
                    .Select(c => new CategoryTreeDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        Children = BuildTree(c.Id)
                    })];
            }

            var tree = BuildTree(null);
            return Result<List<CategoryTreeDto>>.Success(tree);
        }
    }
}