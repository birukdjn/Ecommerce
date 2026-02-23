using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;


namespace Application.Features.Addresses.Commands.CreateAddress
{
    internal class CreateAddressCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<CreateAddressCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
                return Result<Guid>.Failure("User not authenticated.");

            var repo = unitOfWork.Repository<Address>();

            var existingAddress = await repo.GetWithDeletedAsync(a =>
                a.UserId == userId.Value &&
                a.Country.Trim().ToLower() == request.Country.Trim().ToLower() &&
                a.Region.Trim().ToLower() == request.Region.Trim().ToLower() &&
                a.City.Trim().ToLower() == request.City.Trim().ToLower() &&
                a.SpecialPlaceName.Trim().ToLower() == request.SpecialPlaceName.Trim().ToLower());

            if (existingAddress != null)
            {
                if (!existingAddress.IsDeleted)
                    return Result<Guid>.Failure("Address already added.");

                existingAddress.IsDeleted = false;
                existingAddress.IsDefault = request.IsDefault;
                existingAddress.LastModifiedAt = DateTime.UtcNow;
                repo.Update(existingAddress);
            }
            else
            {
                existingAddress = new Address
                {
                    UserId = userId.Value,
                    Country = request.Country,
                    Region = request.Region,
                    City = request.City,
                    SpecialPlaceName = request.SpecialPlaceName,
                    IsDefault = request.IsDefault
                };
                repo.Add(existingAddress);
            }

            if (request.IsDefault)
            {
                var otherDefault = await repo.GetWithDeletedAsync(a =>
                    a.UserId == userId.Value && a.IsDefault && a.Id != existingAddress.Id);

                if (otherDefault != null)
                {
                    otherDefault.IsDefault = false;
                    repo.Update(otherDefault);
                }
            }

            var result = await unitOfWork.Complete();

            return result > 0
                ? Result<Guid>.Success(existingAddress.Id)
                : Result<Guid>.Failure("Failed to save address.");
        }
    }
}
