using Application.DTOs.Category;
using Domain.Common;
using MediatR;

namespace Application.Features.Categories.Queries.GetCategories
{
    public record GetCategoriesQuery : IRequest<Result<List<CategoryDto>>>;
}