namespace Backtrack.Core.Application.Usecases.Admin.GetPostMonthly;

public sealed record PostMonthlyResult(
    string Month,
    int    Year,
    int    Lost,
    int    Found
);
