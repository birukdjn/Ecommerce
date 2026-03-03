using Domain.Common;
using MediatR;
using Domain.Common.Interfaces;
using Domain.Entities;

namespace Application.Features.Addresses.Commands.RestoreAddress
{
    internal class RestoreAddressHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<RestoreAddressCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(RestoreAddressCommand command, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
                return Result<Guid>.Failure("User not authenticated.");

            var repo = unitOfWork.Repository<Address>();

            var existingAddress = await repo.GetWithDeletedAsync(a =>
                a.UserId == userId.Value && a.Id == command.Id
                );
            if (existingAddress == null)
                return Result<Guid>.Failure("Address not found in your deleted list");

            if (!existingAddress.IsDeleted)
                return Result<Guid>.Failure("Address already Restored.");

            existingAddress.IsDeleted = false;
            existingAddress.LastModifiedAt = DateTime.UtcNow;
            repo.Update(existingAddress);

            var result = await unitOfWork.Complete();

            return result > 0
                ? Result<Guid>.Success(existingAddress.Id)
                : Result<Guid>.Failure("Failed to Restore address.");
        }
    }
}
