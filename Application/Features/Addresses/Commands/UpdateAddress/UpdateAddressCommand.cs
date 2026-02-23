using Domain.Common;
using MediatR;


namespace Application.Features.Addresses.Commands.UpdateAddress
{
    public record UpdateAddressCommand(
    Guid Id,
    string Country,
    string Region,
    string City,
    string SpecialPlaceName,
    bool IsDefault) : IRequest<Result<Guid>>;
}
