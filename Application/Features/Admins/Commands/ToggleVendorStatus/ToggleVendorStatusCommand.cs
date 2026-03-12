using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Commands.ToggleVendorStatus
{
    public record ToggleVendorStatusCommand(Guid VendorId, bool IsActive) : IRequest<Result<bool>>;
}