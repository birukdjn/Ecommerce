using Application.DTOs.Address;
using Domain.Common;
using MediatR;

namespace Application.Features.Addresses.Queries.GetAddressById
{
    public record GetAddressByIdQuery(Guid Id) : IRequest<Result<AddressDto>>;

}