using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Vendors.Commands.ApproveVendor
{
    public record ApproveVendorCommand(Guid VendorId) : IRequest<Result<bool>>;

}
