using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Application.DTOs.Admin;
using Application.Interfaces;

namespace Application.Features.Admins.Queries.GetPlatformFinanceSummary
{

    public class GetPlatformFinanceSummaryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetPlatformFinanceSummaryQuery, Result<PlatformFinanceSummaryDto>>
    {
        public async Task<Result<PlatformFinanceSummaryDto>> Handle(GetPlatformFinanceSummaryQuery request, CancellationToken cancellationToken)
        {
            var subOrders = await unitOfWork.Repository<SubOrder>().Query().ToListAsync(cancellationToken);

            var summary = new PlatformFinanceSummaryDto(
                TotalSales: subOrders.Sum(s => s.SubTotal),
                TotalCommissionEarned: subOrders.Where(s => s.Status == SubOrderStatus.Delivered).Sum(s => s.PlatformCommission),
                ActiveEscrowBalance: subOrders.Where(s => s.Status == SubOrderStatus.Pending).Sum(s => s.SubTotal),
                TotalPayoutsProcessed: await unitOfWork.Repository<PayoutRequest>()
                    .Query().Where(p => p.Status == PayoutRequestStatus.Completed).SumAsync(p => p.Amount, cancellationToken)
            );

            return Result<PlatformFinanceSummaryDto>.Success(summary);
        }
    }
}