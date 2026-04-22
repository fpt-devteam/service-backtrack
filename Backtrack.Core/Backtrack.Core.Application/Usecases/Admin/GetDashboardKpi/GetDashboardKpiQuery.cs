using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetDashboardKpi;

public sealed record GetDashboardKpiQuery(string AdminUserId) : IRequest<DashboardKpiResult>;
