using Application.DTOs.Address;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Addresses.Queries.GetAddresses
{
    public class GetAddressesHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<GetAddressesQuery, Result<IReadOnlyList<AddressDto>>>
    {
        public async Task<Result<IReadOnlyList<AddressDto>>> Handle(GetAddressesQuery request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
                return Result<IReadOnlyList<AddressDto>>.Failure("Unauthorized.");

            var repo = unitOfWork.Repository<Address>();

            var addresses = await repo.Query()
                .AsNoTracking()
                .Where(a => a.UserId == userId.Value)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync(cancellationToken);

            var dto = addresses.Select(a => new AddressDto
            {
                Id = a.Id,
                Country = a.Country,
                Region = a.Region,
                City = a.City,
                SpecialPlaceName = a.SpecialPlaceName,
                IsDefault = a.IsDefault
            }).ToList();
            return Result<IReadOnlyList<AddressDto>>.Success(dto);
        }

    }
}
