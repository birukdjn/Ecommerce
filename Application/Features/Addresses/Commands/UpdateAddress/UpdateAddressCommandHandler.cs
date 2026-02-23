using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Addresses.Commands.UpdateAddress
{
    public class UpdateAddressCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<UpdateAddressCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetCurrentUserId();
            var repo = unitOfWork.Repository<Address>();

            var address = await repo.GetByIdAsync(request.Id);

            if (address == null || address.UserId != userId)
                return Result<Guid>.Failure("Address not found or unauthorized.");

            if (request.IsDefault && !address.IsDefault)
            {
                var all = await repo.ListAllAsync();
                var others = all.Where(a => a.UserId == userId && a.Id != address.Id);
                foreach (var other in others)
                {
                    other.IsDefault = false;
                    repo.Update(other);
                }
            }

            address.Country = request.Country;
            address.Region = request.Region;
            address.City = request.City;
            address.SpecialPlaceName = request.SpecialPlaceName;
            address.IsDefault = request.IsDefault;

            repo.Update(address);
            var result = await unitOfWork.Complete();

            return result > 0
                ? Result<Guid>.Success(address.Id)
                : Result<Guid>.Failure("No changes were saved.");
        }

    }
}
