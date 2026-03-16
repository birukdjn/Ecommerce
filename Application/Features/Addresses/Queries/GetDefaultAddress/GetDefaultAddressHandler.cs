using Application.DTOs.Address;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Addresses.Queries.GetDefaultAddress
{
    public class GetDefaultAddressHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<GetDefaultAddressQuery, Result<AddressDto>>
    {
        public async Task<Result<AddressDto>> Handle(GetDefaultAddressQuery request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
                return Result<AddressDto>.Failure("Unauthorized");

            var repo = unitOfWork.Repository<Address>();

            var address = await repo.Query()
                .AsNoTracking()
                .Where(a => a.UserId == userId && a.IsDefault)
                .Select(a => new AddressDto
                {
                    Id = a.Id,
                    Country = a.Country,
                    Region = a.Region,
                    City = a.City,
                    SpecialPlaceName = a.SpecialPlaceName,
                    IsDefault = a.IsDefault
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (address == null)
                return Result<AddressDto>.Failure("No default address found.");

            return Result<AddressDto>.Success(address);
        }
    }
}