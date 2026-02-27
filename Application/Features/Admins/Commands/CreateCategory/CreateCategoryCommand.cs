using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Commands.CreateCategory
{
    public record CreateCategoryCommand
    (
        string Name, 
        string? Description) : IRequest<Result<Guid>>;
}