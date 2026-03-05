using Application.DTOs.Address;
using Domain.Common;
using MediatR;

namespace Application.Features.Addresses.Queries.GetAddresses
{
    public record GetAddressesQuery() : IRequest<Result<IReadOnlyList<AddressDto>>>;
}
