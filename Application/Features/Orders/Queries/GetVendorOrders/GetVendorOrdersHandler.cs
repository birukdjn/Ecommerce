using Application.DTOs.Order;
using Domain.Common;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces;

namespace Application.Features.Orders.Queries.GetVendorOrders
{
    public class GetVendorOrdersHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        : IRequestHandler<GetVendorOrdersQuery, Result<List<VendorOrderDto>>>
    {
        public async Task<Result<List<VendorOrderDto>>> Handle(GetVendorOrdersQuery request, CancellationToken cancellationToken)
        {
            if (!currentUserService.IsVendor())
                return Result<List<VendorOrderDto>>.Failure("Unauthorized");

            var vendorId = currentUserService.GetCurrentVendorId();
            if (vendorId == null) return Result<List<VendorOrderDto>>.Failure("User is not a vendor.");

            var subOrders = await unitOfWork.Repository<SubOrder>().Query()
                .Include(s => s.MasterOrder).ThenInclude(m => m.Customer)
                .Include(s => s.Items).ThenInclude(i => i.Product)
                .Where(s => s.VendorId == vendorId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new VendorOrderDto(
                    s.Id,
                    s.MasterOrder.OrderNumber,
                    s.SubTotal,
                    s.PlatformCommission,
                    s.SubTotal - s.PlatformCommission,
                    s.Status.ToString(),
                    s.CreatedAt,
                    s.MasterOrder.Customer.FullName ?? "Customer",
                    s.Items.Select(i => new OrderItemDto(
                        i.ProductId,
                        i.Product.Name,
                        i.Quantity,
                        i.UnitPrice,
                        null
                    )).ToList()
                ))
                .ToListAsync(cancellationToken);

            return Result<List<VendorOrderDto>>.Success(subOrders);
        }
    }
}