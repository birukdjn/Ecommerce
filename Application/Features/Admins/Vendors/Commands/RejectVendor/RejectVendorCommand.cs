using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Vendors.Commands.RejectVendor
{

    public record RejectVendorCommand(Guid VendorId, string Reason) : IRequest<Result<bool>>;

}
