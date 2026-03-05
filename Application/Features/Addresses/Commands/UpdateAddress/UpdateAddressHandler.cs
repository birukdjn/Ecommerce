using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Addresses.Commands.UpdateAddress
{
    public class UpdateAddressHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<UpdateAddressCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(UpdateAddressCommand command, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();

            if (userId == null || userId == Guid.Empty)
                return Result<Guid>.Failure("User not authenticated.");

            var repo = unitOfWork.Repository<Address>();

            var address = await repo.Query()
              .FirstOrDefaultAsync(a => a.Id == command.Id && a.UserId == userId, cancellationToken);

            if (address == null)
                return Result<Guid>.Failure("Address not found.");

            if (command.IsDefault && !address.IsDefault)
            {
                var others = await repo.Query()
                    .Where(a => a.UserId == userId && a.Id != address.Id)
                    .ToListAsync(cancellationToken);

                foreach (var other in others)
                {
                    other.IsDefault = false;
                    repo.Update(other);
                }
            }

            address.Country = command.Country;
            address.Region = command.Region;
            address.City = command.City;
            address.SpecialPlaceName = command.SpecialPlaceName;
            address.IsDefault = command.IsDefault;

            repo.Update(address);
            var result = await unitOfWork.Complete();

            return result > 0
                ? Result<Guid>.Success(address.Id)
                : Result<Guid>.Failure("No changes were saved.");
        }

    }
}
