using Application.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Admins.Queries.GetUsers
{
    public record GetAllUsersQuery : IRequest<Result<List<UserSummaryDto>>>;

}