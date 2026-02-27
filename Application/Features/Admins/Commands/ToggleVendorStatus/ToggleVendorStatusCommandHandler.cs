using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Domain.Enums;

namespace Application.Features.Admins.Commands.ToggleVendorStatus
{

    public class ToggleVendorStatusCommandHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<ToggleVendorStatusCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(ToggleVendorStatusCommand request, CancellationToken ct)
        {
            var vendorRepo = unitOfWork.Repository<Vendor>();

            var vendor = await vendorRepo.GetByIdAsync(request.VendorId);

            if (vendor == null)
                return Result<bool>.Failure("Vendor not found.");

            vendor.Status = request.IsActive
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
