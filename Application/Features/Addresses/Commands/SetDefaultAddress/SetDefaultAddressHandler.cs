using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Addresses.Commands.SetDefaultAddress
{
    public class SetDefaultAddressHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<SetDefaultAddressCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(SetDefaultAddressCommand command, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
                return Result<bool>.Failure("User not authenticated.");

            var repo = unitOfWork.Repository<Address>();

            // Fetch all addresses for the user
            var userAddresses = await repo.Query()
                .Where(a => a.UserId == userId)
                .ToListAsync(cancellationToken);

            if (!userAddresses.Any(a => a.Id == command.Id))
                return Result<bool>.Failure("Address not found.");

            // Set the selected address as default, others as not default
            foreach (var address in userAddresses)
            {
                address.IsDefault = address.Id == command.Id;
                repo.Update(address);
            }

            var result = await unitOfWork.Complete();

            return result > 0
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("Failed to update default address.");
        }
    }
}