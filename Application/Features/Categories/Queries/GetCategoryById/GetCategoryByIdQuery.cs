using Application.DTOs.Category;
using Domain.Common;
using MediatR;

namespace Application.Features.Categories.Queries.GetCategoryById
{
    public record GetCategoryByIdQuery(Guid Id) : IRequest<Result<CategoryDto>>;
}