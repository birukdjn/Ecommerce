using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Addresses.Commands.DeleteAddress
{
    public class DeleteAddressCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<DeleteAddressCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteAddressCommand request, CancellationToken ct)
        {
            var userId = currentUserService.GetCurrentUserId();
            var repo = unitOfWork.Repository<Address>();

            var address = await repo.GetByIdAsync(request.Id);

            if (address == null || address.UserId != userId)
                return Result<bool>.Failure("Address not found or unauthorized.");

            repo.Delete(address);

            return await unitOfWork.Complete() > 0
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("Failed to delete address.");
        }
    }
}