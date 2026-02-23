using Domain.Common;
using Domain.Entities;
using MediatR;

namespace Application.Features.Addresses.Queries.GetAddresses
{
    public record GetAddressesQuery() : IRequest<Result<IReadOnlyList<Address>>>;
    public record GetAddressByIdQuery(Guid Id) : IRequest<Result<Address>>;
}
