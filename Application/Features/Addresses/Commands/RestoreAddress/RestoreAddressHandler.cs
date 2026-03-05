using Domain.Common;
using MediatR;
using Domain.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Addresses.Commands.RestoreAddress
{
    public class RestoreAddressHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<RestoreAddressCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(RestoreAddressCommand command, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();

            if (userId == null || userId == Guid.Empty)
                return Result<Guid>.Failure("User not authenticated.");

            var repo = unitOfWork.Repository<Address>();

            var existingAddress = await repo.Query()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Id == command.Id &&
                                          a.UserId == userId, cancellationToken);

            if (existingAddress == null)
                return Result<Guid>.Failure("Address not found in your deleted list.");

            if (!existingAddress.IsDeleted)
                return Result<Guid>.Failure("Address is already restored.");

            existingAddress.IsDeleted = false;
            existingAddress.LastModifiedAt = DateTime.UtcNow;

            repo.Update(existingAddress);

            var result = await unitOfWork.Complete();

            return result > 0
                ? Result<Guid>.Success(existingAddress.Id)
                : Result<Guid>.Failure("Failed to restore address.");
        }
    }
}