using Application.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Vendors.Queries.GetVendorRequestById
{
    public record GetVendorRequestByIdQuery(Guid Id) : IRequest<Result<VendorRequestDetailsDto>>;
}
