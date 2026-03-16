using Application.DTOs.Admin;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces;

namespace Application.Features.Admins.Queries.GetPendingPayouts
{

    public class GetPendingPayoutsHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetPendingPayoutsQuery, Result<List<PayoutRequestDto>>>
    {
        public async Task<Result<List<PayoutRequestDto>>> Handle(GetPendingPayoutsQuery request, CancellationToken cancellationToken)
        {
            var pendingRequests = await unitOfWork.Repository<PayoutRequest>().Query()
                .Include(p => p.Vendor)
                .Where(p => p.Status == PayoutRequestStatus.Pending)
                .OrderBy(p => p.CreatedAt)
                .Select(p => new PayoutRequestDto(
                    p.Id,
                    p.VendorId,
                    p.Vendor.StoreName,
                    p.Amount,
                    p.Status.ToString(),
                    p.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            return Result<List<PayoutRequestDto>>.Success(pendingRequests);
        }
    }
}