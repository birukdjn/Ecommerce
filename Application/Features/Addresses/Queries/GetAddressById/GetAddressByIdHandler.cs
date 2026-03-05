using Application.DTOs.Address;
using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Addresses.Queries.GetAddressById
{
    public class GetAddressesByIdHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<GetAddressByIdQuery, Result<AddressDto>>
    {
        public async Task<Result<AddressDto>> Handle(GetAddressByIdQuery request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
                return Result<AddressDto>.Failure("User not authenticated");

            var repo = unitOfWork.Repository<Address>();

            var address = await repo.Query().FirstOrDefaultAsync(a => a.Id == request.Id && a.UserId == userId, cancellationToken);

            if (address == null)
                return Result<AddressDto>.Failure("Address not found.");

            var dto = new AddressDto
            {
                Id = address.Id,
                Country = address.Country,
                Region = address.Region,
                City = address.City,
                SpecialPlaceName = address.SpecialPlaceName,
                IsDefault = address.IsDefault
            };

            return Result<AddressDto>.Success(dto);
        }
    }
}
