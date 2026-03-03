using Domain.Common;
using Domain.Entities;
using MediatR;

namespace Application.Features.Addresses.Queries.GetAddressById
{
    public record GetAddressByIdQuery(Guid Id) : IRequest<Result<Address>>;

}