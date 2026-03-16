using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Application.Templates.Email;
using Microsoft.EntityFrameworkCore;
using Domain.Enums;

namespace Application.Features.Admins.Commands.RejectProduct
{
    public class RejectProductHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IJobService jobService) : IRequestHandler<RejectProductCommand, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(RejectProductCommand command, CancellationToken cancellationToken)
        {
            if (!currentUserService.IsAdmin())
                return Result<Unit>.Failure("Unauthorized");

            var productRepo = unitOfWork.Repository<Product>();

            var product = await productRepo.GetByIdAsync(command.ProductId);
            if (product == null) return Result<Unit>.Failure("Product not found");

            if (product.Status == ProductStatus.Rejected)
                return Result<Unit>.Failure("Product Already Rejected");

            product.Status = ProductStatus.Rejected;
            product.RejectionReason = command.Reason;

            productRepo.Update(product);
            await unitOfWork.Complete();

            var vendorRepo = unitOfWork.Repository<Vendor>();
            var vendor = await vendorRepo.Query()
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == product.VendorId, cancellationToken);

            // 3. Enqueue the background email
            if (vendor?.User != null && !string.IsNullOrEmpty(vendor.User.Email))
            {
                jobService.Enqueue<IEmailSender>(sender =>
                    sender.SendEmailAsync(
                        vendor.User.Email,
                        $"Action Required: Product Rejected - {product.Name}",
                        EmailTemplates.GetProductRejectedEmail(
                            vendor.User.FullName ?? vendor.StoreName,
                            product.Name,
                            command.Reason)
                    ));
            }
            return Result<Unit>.Success(Unit.Value);
        }
    }
}