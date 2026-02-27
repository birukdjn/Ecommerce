using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Commands.DeleteCategory
{
public record DeleteCategoryCommand(Guid Id) : IRequest<Result<bool>>;
}