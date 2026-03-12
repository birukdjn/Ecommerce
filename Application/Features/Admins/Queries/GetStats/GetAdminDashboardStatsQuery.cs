using Application.DTOs.Admin;
using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Queries.GetStats
{
    public record GetAdminDashboardStatsQuery : IRequest<Result<AdminStatsDto>>;
}