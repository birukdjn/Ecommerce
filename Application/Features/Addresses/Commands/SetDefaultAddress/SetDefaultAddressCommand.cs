using Domain.Common;
using MediatR;

namespace Application.Features.Addresses.Commands.SetDefaultAddress
{
    public record SetDefaultAddressCommand(Guid Id) : IRequest<Result<bool>>;

}
