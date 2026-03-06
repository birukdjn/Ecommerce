using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Admins.Products.Commands.RejectProduct
{
    public class RejectProductHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<RejectProductCommand, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(RejectProductCommand command, CancellationToken cancellationToken)
        {
            if (!currentUserService.IsAdmin())
                return Result<Unit>.Failure("Unauthorized");

            var productRepo = unitOfWork.Repository<Product>();

            var product = await productRepo.GetByIdAsync(command.ProductId);
            if (product == null) return Result<Unit>.Failure("Product not found");

            product.IsApproved = false;
            product.RejectionReason = command.Reason;

            productRepo.Update(product);
            await unitOfWork.Complete();
            return Result<Unit>.Success(Unit.Value);
        }
    }
}