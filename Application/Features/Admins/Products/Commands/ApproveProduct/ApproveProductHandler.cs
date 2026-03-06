using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Admins.Products.Commands.ApproveProduct;

public class ApproveProductHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ApproveProductCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ApproveProductCommand command, CancellationToken cancellationToken)
    {
        var product = await unitOfWork.Repository<Product>().GetByIdAsync(command.ProductId);

        if (product == null)
            return Result<Unit>.Failure("Product not found");

        product.IsApproved = true;

        unitOfWork.Repository<Product>().Update(product);
        await unitOfWork.Complete();

        return Result<Unit>.Success(Unit.Value);
    }
}