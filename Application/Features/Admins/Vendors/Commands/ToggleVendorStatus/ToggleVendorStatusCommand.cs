using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Vendors.Commands.ToggleVendorStatus
{
    public record ToggleVendorStatusCommand(Guid VendorId, bool IsActive) : IRequest<Result<bool>>;
}