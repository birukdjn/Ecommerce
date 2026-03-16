using Application.Interfaces;
using Application.Templates.Email;
using Microsoft.EntityFrameworkCore;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Domain.Enums;

namespace Application.Features.Admins.Commands.ApproveProduct;

public class ApproveProductHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IJobService jobService)
    : IRequestHandler<ApproveProductCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ApproveProductCommand command, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAdmin())
            return Result<Unit>.Failure("Unauthorized");

        var productRepo = unitOfWork.Repository<Product>();
        var product = await productRepo.GetByIdAsync(command.ProductId);

        if (product == null)
            return Result<Unit>.Failure("Product not found");

        if (product.Status == ProductStatus.Approved)
            return Result<Unit>.Failure("Product Already Approved");

        product.Status = ProductStatus.Approved;

        productRepo.Update(product);
        await unitOfWork.Complete();


        var vendorRepo = unitOfWork.Repository<Vendor>();
        var vendor = await vendorRepo.Query()
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.Id == product.VendorId, cancellationToken);

        // Send Background Email to Vendor
        if (vendor?.User != null && !string.IsNullOrEmpty(vendor.User.Email))
        {
            jobService.Enqueue<IEmailSender>(sender =>
                sender.SendEmailAsync(
                    vendor.User.Email,
                    "Product Approved: " + product.Name,
                    EmailTemplates.GetProductApprovedEmail(vendor.User.FullName ?? vendor.StoreName, product.Name)
                ));
        }

        return Result<Unit>.Success(Unit.Value);
    }
}