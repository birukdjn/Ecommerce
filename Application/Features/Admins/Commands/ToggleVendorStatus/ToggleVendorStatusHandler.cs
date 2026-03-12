using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Domain.Enums;

namespace Application.Features.Admins.Commands.ToggleVendorStatus
{

    public class ToggleVendorStatusHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<ToggleVendorStatusCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(ToggleVendorStatusCommand command, CancellationToken cancellationToken)
        {
            var vendorRepo = unitOfWork.Repository<Vendor>();

            var vendor = await vendorRepo.GetByIdAsync(command.VendorId);

            if (vendor == null)
                return Result<bool>.Failure("Vendor not found.");

            vendor.Status = command.IsActive
                ? VendorStatus.Active
                : VendorStatus.Suspended;

            vendorRepo.Update(vendor);

            var result = await unitOfWork.Complete();

            return result > 0
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("Failed to update vendor status.");
        }
    }
}
