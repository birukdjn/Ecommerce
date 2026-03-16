using Application.DTOs.Admin;
using Application.Interfaces;
using Domain.Common;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admins.Queries.GetStats
{
    public class GetAdminDashboardStatsHandler(IApplicationDbContext context)
    : IRequestHandler<GetAdminDashboardStatsQuery, Result<AdminStatsDto>>
    {
        public async Task<Result<AdminStatsDto>> Handle(GetAdminDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            // Execute these one by one to avoid EF Core Concurrency exceptions
            var totalUsers = await context.Users.AsNoTracking().CountAsync(cancellationToken);
            var activeUsers = await context.Users.AsNoTracking().CountAsync(u => u.IsActive, cancellationToken);
            var totalVendors = await context.Vendors.AsNoTracking().CountAsync(cancellationToken);
            var pendingVendors = await context.Vendors.AsNoTracking().CountAsync(v => v.Status == VendorStatus.Pending, cancellationToken);
            var activeVendors = await context.Vendors.AsNoTracking().CountAsync(v => v.Status == VendorStatus.Active, cancellationToken);
            var totalOrders = await context.Orders.AsNoTracking().CountAsync(cancellationToken);
            var totalProducts = await context.Products.AsNoTracking().CountAsync(cancellationToken);
            var totalCategories = await context.Categories.AsNoTracking().CountAsync(cancellationToken);

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