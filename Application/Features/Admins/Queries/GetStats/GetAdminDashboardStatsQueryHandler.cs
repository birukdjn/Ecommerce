using Application.DTOs;
using Domain.Common;
using Domain.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admins.Queries.GetStats
{
    public class GetAdminDashboardStatsHandler(IApplicationDbContext context)
    : IRequestHandler<GetAdminDashboardStatsQuery, Result<AdminStatsDto>>
    {
        public async Task<Result<AdminStatsDto>> Handle(GetAdminDashboardStatsQuery request, CancellationToken ct)
        {
            // Execute these one by one to avoid EF Core Concurrency exceptions
            var totalUsers = await context.Users.CountAsync(ct);
            var activeUsers = await context.Users.CountAsync(u => u.IsActive, ct);
            var totalVendors = await context.Vendors.CountAsync(ct);
            var pendingVendors = await context.Vendors.CountAsync(v => v.Status == VendorStatus.Pending, ct);
            var activeVendors = await context.Vendors.CountAsync(v => v.Status == VendorStatus.Active, ct);
            var totalOrders = await context.Orders.CountAsync(ct);
            var totalProducts = await context.Products.CountAsync(ct);
            var totalCategories = await context.Categories.CountAsync(ct);

            return Result<AdminStatsDto>.Success(new AdminStatsDto(
                totalUsers,
                activeUsers,
                totalVendors,
                pendingVendors,
                activeVendors,
                totalOrders,
                totalProducts,
                totalCategories
            ));
        }
    }
}