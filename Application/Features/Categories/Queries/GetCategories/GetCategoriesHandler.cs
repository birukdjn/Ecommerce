using Application.DTOs.Category;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Categories.Queries.GetCategories
{
    public class GetCategoriesHandler(
        IUnitOfWork unitOfWork)
        : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
    {
        public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categoryRepo = unitOfWork.Repository<Category>();
            var categories = await categoryRepo.Query()
                .AsNoTracking()
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                })
                .ToListAsync(cancellationToken);

            return Result<List<CategoryDto>>.Success(categories);
        }
    }
}