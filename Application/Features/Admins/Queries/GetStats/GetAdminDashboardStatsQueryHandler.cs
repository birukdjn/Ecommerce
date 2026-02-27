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
            var totalUsersTask = context.Users.CountAsync(ct);
            var activeUsersTask = context.Users.CountAsync(u => u.IsActive, ct);
            var totalVendorsTask = context.Vendors.CountAsync(ct);
            var pendingVendorsTask = context.Vendors.CountAsync(v => v.Status == VendorStatus.Pending, ct);
            var activeVendorsTask = context.Vendors.CountAsync(v => v.Status == VendorStatus.Active, ct);
            var totalOrdersTask = context.Orders.CountAsync(ct);
            var totalProductsTask = context.Products.CountAsync(ct);
            var totalCategoriesTask = context.Categories.CountAsync(ct);

            await Task.WhenAll(totalUsersTask, activeUsersTask, totalVendorsTask, pendingVendorsTask,
                                   activeVendorsTask, totalOrdersTask, totalProductsTask, totalCategoriesTask);


            return Result<AdminStatsDto>.Success(new AdminStatsDto(
                totalUsersTask.Result,
                activeUsersTask.Result,
                totalVendorsTask.Result,
                pendingVendorsTask.Result,
                activeVendorsTask.Result,
                totalOrdersTask.Result,
                totalProductsTask.Result,
                totalCategoriesTask.Result
            ));
        }
    }
}