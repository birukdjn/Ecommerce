using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Addresses.Queries.GetAddresses
{
    public class GetAddressesQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService) 
    : IRequestHandler<GetAddressesQuery, Result<IReadOnlyList<Address>>>,
      IRequestHandler<GetAddressByIdQuery, Result<Address>>
{
    public async Task<Result<IReadOnlyList<Address>>> Handle(GetAddressesQuery request, CancellationToken ct)
        {
            var userId = currentUserService.GetCurrentUserId();
            if (userId == null) return Result<IReadOnlyList<Address>>.Failure("Unauthorized.");

            var allAddresses = await unitOfWork.Repository<Address>().ListAllAsync();

            var userAddresses = allAddresses
                .Where(a => a.UserId == userId.Value)
                .OrderByDescending(a => a.IsDefault)
                .ToList();

            return Result<IReadOnlyList<Address>>.Success(userAddresses);
        }

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
