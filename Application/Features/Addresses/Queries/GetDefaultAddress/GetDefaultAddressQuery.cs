using Application.DTOs.Address;
using Domain.Common;
using MediatR;

namespace Application.Features.Addresses.Queries.GetDefaultAddress
{
    public record GetDefaultAddressQuery() : IRequest<Result<AddressDto>>;
}