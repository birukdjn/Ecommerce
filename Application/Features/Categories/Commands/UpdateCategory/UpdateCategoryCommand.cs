using Domain.Common;
using MediatR;

namespace Application.Features.Categories.Commands.UpdateCategory
{
    public record UpdateCategoryCommand(Guid Id, string Name, string? Description) : IRequest<Result<bool>>;
}