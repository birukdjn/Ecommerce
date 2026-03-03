using Application.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Categories.Queries.GetCategoryTree
{
    public record GetCategoryTreeQuery : IRequest<Result<List<CategoryDto>>>;
}