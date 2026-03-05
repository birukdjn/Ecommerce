using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Addresses.Commands.CreateAddress
{
    internal class CreateAddressHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<CreateAddressCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateAddressCommand command, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();

            if (userId == null || userId == Guid.Empty)
                return Result<Guid>.Failure("User not authenticated.");

            var repo = unitOfWork.Repository<Address>();

            // Normalize input for duplicate comparison
            var country = command.Country.Trim().ToLower();
            var region = command.Region.Trim().ToLower();
            var city = command.City.Trim().ToLower();
            var specialPlace = command.SpecialPlaceName.Trim().ToLower();

            // Check duplicate INCLUDING deleted addresses
            var existingAddress = await repo.Query()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a =>
                    a.UserId == userId.Value &&
                    a.Country.ToLower() == country &&
                    a.Region.ToLower() == region &&
                    a.City.ToLower() == city &&
                    a.SpecialPlaceName.ToLower() == specialPlace,
                    cancellationToken);

            if (existingAddress != null)
            {
                // Already exists and not deleted
                if (!existingAddress.IsDeleted)
                    return Result<Guid>.Failure("Address already added.");

                // Restore deleted address
                existingAddress.IsDeleted = false;
                existingAddress.IsDefault = command.IsDefault;
                existingAddress.LastModifiedAt = DateTime.UtcNow;

                repo.Update(existingAddress);
            }
            else
            {
                // Create new address
                existingAddress = new Address
                {
                    UserId = userId.Value,
                    Country = command.Country.Trim(),
                    Region = command.Region.Trim(),
                    City = command.City.Trim(),
                    SpecialPlaceName = command.SpecialPlaceName.Trim(),
                    IsDefault = command.IsDefault
                };

                repo.Add(existingAddress);
            }

            // Auto-set first address as default
            var hasAnyAddress = await repo.Query()
                .AnyAsync(a => a.UserId == userId.Value && !a.IsDeleted, cancellationToken);

            if (!hasAnyAddress)
            {
                existingAddress.IsDefault = true;
            }

            // Ensure only one default address per user
            if (existingAddress.IsDefault)
            {
                var otherDefaults = await repo.Query()
                    .Where(a => a.UserId == userId.Value &&
                                a.IsDefault &&
                                a.Id != existingAddress.Id)
                    .ToListAsync(cancellationToken);

                foreach (var other in otherDefaults)
                {
                    other.IsDefault = false;
                    repo.Update(other);
                }
            }

            var result = await unitOfWork.Complete();

            return result > 0
                ? Result<Guid>.Success(existingAddress.Id)
                : Result<Guid>.Failure("Failed to save address.");
        }
    }
}