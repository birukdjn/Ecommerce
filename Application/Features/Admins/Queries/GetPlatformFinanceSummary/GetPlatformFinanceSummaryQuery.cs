using Application.DTOs.Admin;
using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Queries.GetPlatformFinanceSummary
{
    public record GetPlatformFinanceSummaryQuery : IRequest<Result<PlatformFinanceSummaryDto>>;

}