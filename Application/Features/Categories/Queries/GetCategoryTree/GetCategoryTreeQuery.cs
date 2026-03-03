using Application.DTOs;
using MediatR;

namespace Application.Features.Categories.Queries.GetCategoryTree
{
    public record GetCategoryTreeQuery : IRequest<List<CategoryDto>>;
}