using Domain.Common;
using MediatR;

namespace Application.Features.Addresses.Commands.CreateAddress
{
    public record CreateAddressCommand(
     string Country,
     string Region,
     string City,
     string SpecialPlaceName,
     bool IsDefault) : IRequest<Result<Guid>>;
}
