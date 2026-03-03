using Domain.Common;
using MediatR;
namespace Application.Features.Addresses.Commands.RestoreAddress
{
    public record RestoreAddressCommand
    (
        Guid Id
     ) : IRequest<Result<Guid>>;
}
