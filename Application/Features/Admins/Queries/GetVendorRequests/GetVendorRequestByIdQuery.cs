using Application.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Queries.GetVendorRequests
{
    public record GetVendorRequestByIdQuery(Guid Id) : IRequest<Result<VendorRequestDetailsDto>>;
}
