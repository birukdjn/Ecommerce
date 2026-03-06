using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Admins.Products.Commands.ApproveProduct;

public class ApproveProductHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
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

        product.IsApproved = true;

        productRepo.Update(product);
        await unitOfWork.Complete();

        return Result<Unit>.Success(Unit.Value);
    }
}