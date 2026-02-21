using Domain.Common;
using MediatR;


namespace Application.Features.Vendors.Commands.BecomeVendor
{
    public record BecomeVendorCommand(
     string StoreName,
     string? Description,
     string? LogoUrl) : IRequest<Result<Guid>>;
}
