using Domain.Common;
using MediatR;

namespace Application.Features.Categories.Commands.CreateCategory
{
    public record CreateCategoryCommand
    (
        string Name,
        string? Description,
        Guid? ParentCategoryId,
        decimal CommissionPercentage
    ) : IRequest<Result<Guid>>;
}