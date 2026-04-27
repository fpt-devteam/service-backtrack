namespace Backtrack.Core.Application.Usecases.Admin.GetOrgMonthlyActivity;

public sealed record MonthlyActivityPoint(string Month, int Lost, int Found, int Returned);
