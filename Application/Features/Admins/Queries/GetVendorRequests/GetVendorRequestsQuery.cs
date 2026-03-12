using Application.DTOs.Admin;
using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Queries.GetVendorRequests
{
    public record GetVendorRequestsQuery : IRequest<Result<List<VendorRequestDetailsDto>>>;
}
