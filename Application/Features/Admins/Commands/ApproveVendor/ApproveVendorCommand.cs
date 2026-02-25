
using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Commands.ApproveVendor
{
    public record ApproveVendorCommand(Guid VendorId) : IRequest<Result<bool>>;
    
}
