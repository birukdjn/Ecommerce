using Application.DTOs.Admin;
using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Queries.GetVendors
{
    public record GetAllVendorsQuery : IRequest<Result<List<VendorSummaryDto>>>;

}