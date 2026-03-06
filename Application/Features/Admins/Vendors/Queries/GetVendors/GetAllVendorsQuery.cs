using Application.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Queries.GetVendors
{
    public record GetAllVendorsQuery : IRequest<Result<List<VendorSummaryDto>>>;

}