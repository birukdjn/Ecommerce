using Domain.Common;
using MediatR;

namespace Application.Features.Categories.Commands.CreateCategory
{
    public record CreateCategoryCommand
    (
        string Name,
        string? Description) : IRequest<Result<Guid>>;
}