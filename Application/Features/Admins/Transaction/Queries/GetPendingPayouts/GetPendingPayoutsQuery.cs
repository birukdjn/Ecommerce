using Application.DTOs.Admin;
using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Transaction.Queries.GetPendingPayouts
{
    public record GetPendingPayoutsQuery : IRequest<Result<List<PayoutRequestDto>>>;
}