using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetDashboardOverview;

public sealed record GetDashboardOverviewQuery(string AdminUserId) : IRequest<DashboardOverviewResult>;
