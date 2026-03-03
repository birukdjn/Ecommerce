using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Addresses.Queries.GetAddressById
{
    public class GetAddressesByIdHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<GetAddressByIdQuery, Result<Address>>
    {
        public async Task<Result<Address>> Handle(GetAddressByIdQuery request, CancellationToken ct)
        {
            var userId = currentUserService.GetCurrentUserId();
            var address = await unitOfWork.Repository<Address>().GetByIdAsync(request.Id);

            if (address == null || address.UserId != userId)
                return Result<Address>.Failure("Address not found.");

            return Result<Address>.Success(address);
        }
    }
}
