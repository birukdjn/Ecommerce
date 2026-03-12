using Application.DTOs.Admin;
using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Queries.GetPendingPayouts
{
    public record GetPendingPayoutsQuery : IRequest<Result<List<PayoutRequestDto>>>;
}