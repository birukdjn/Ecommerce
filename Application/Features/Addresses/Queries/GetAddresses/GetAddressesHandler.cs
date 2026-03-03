using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Addresses.Queries.GetAddresses
{
    public class GetAddressesHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<GetAddressesQuery, Result<IReadOnlyList<Address>>>
    {
        public async Task<Result<IReadOnlyList<Address>>> Handle(GetAddressesQuery request, CancellationToken ct)
        {
            var userId = currentUserService.GetCurrentUserId();
            if (userId == null) return Result<IReadOnlyList<Address>>.Failure("Unauthorized.");

            var allAddresses = await unitOfWork.Repository<Address>().ListAllAsync();

            var userAddresses = allAddresses
                .Where(a => a.UserId == userId.Value)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .ToList();

            return Result<IReadOnlyList<Address>>.Success(userAddresses);
        }

    }
}
