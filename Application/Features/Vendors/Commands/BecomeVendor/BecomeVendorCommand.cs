using Domain.Common;
using MediatR;

namespace Application.Features.Vendors.Commands.BecomeVendor
{
    public record BecomeVendorCommand : IRequest<Result<Guid>>
    {
        public string StoreName { get; init; } = null!;
        public string? Description { get; init; }
        public string LogoUrl { get; init; } = null!;
        public string LicenseUrl { get; init; } = null!;
        public string PhoneNumber { get; init; } = null!;
    }
}
