using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Addresses.Commands.SetDefaultAddress
{
    public class SetDefaultAddressCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<SetDefaultAddressCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(SetDefaultAddressCommand request, CancellationToken ct)
        {
            var userId = currentUserService.GetCurrentUserId();
            var repo = unitOfWork.Repository<Address>();

            
            var activeAddresses = await unitOfWork.Repository<Address>().ListAllAsync();
            var userAddresses = activeAddresses.Where(a => a.UserId == userId);

            // 2. Update logic
            foreach (var address in userAddresses)
            {
                address.IsDefault = (address.Id == request.Id);
                unitOfWork.Repository<Address>().Update(address);
            }

            return await unitOfWork.Complete() > 0
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("Failed to update default address.");
        }
    }
}
