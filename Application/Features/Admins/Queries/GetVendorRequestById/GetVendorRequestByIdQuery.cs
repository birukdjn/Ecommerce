using Application.DTOs.Admin;
using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Queries.GetVendorRequestById
{
    public record GetVendorRequestByIdQuery(Guid Id) : IRequest<Result<VendorRequestDetailsDto>>;
}
