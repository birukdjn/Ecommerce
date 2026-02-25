using Application.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Queries.GetVendorRequests
{
    public record GetVendorRequestsQuery : IRequest<Result<List<VendorRequestDetailsDto>>>;
}
