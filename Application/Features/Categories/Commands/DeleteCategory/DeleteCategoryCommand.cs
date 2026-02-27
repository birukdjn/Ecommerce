using Domain.Common;
using MediatR;

namespace Application.Features.Categories.Commands.DeleteCategory
{
    public record DeleteCategoryCommand(Guid Id) : IRequest<Result<bool>>;
}