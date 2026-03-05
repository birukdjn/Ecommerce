using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Addresses.Commands.DeleteAddress
{
    public class DeleteAddressHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<DeleteAddressCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteAddressCommand command, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();

            if (userId == null || userId == Guid.Empty)
                return Result<bool>.Failure("User not authenticated.");

            var repo = unitOfWork.Repository<Address>();

            var address = await repo.GetByIdAsync(command.Id);

            if (address == null || address.UserId != userId)
                return Result<bool>.Failure("Address not found or unauthorized.");

            var wasDefault = address.IsDefault;

            repo.Delete(address);

            // If deleted address was default → assign another one
            if (wasDefault)
            {
                var nextAddress = await repo.Query()
                    .Where(a => a.UserId == userId && !a.IsDeleted && a.Id != address.Id)
                    .OrderBy(a => a.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (nextAddress != null)
                {
                    nextAddress.IsDefault = true;
                    repo.Update(nextAddress);
                }
            }

            var result = await unitOfWork.Complete();

            return result > 0
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("Failed to delete address.");
        }
    }
}