using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;

namespace Backtrack.Core.Application.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User, string>
{
    Task<User> EnsureExistAsync(User user);

    Task<(List<User> Items, int Total)> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        UserStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<(int Total, int Active, int NewThisMonth)> GetCountsAsync(CancellationToken cancellationToken = default);

    Task<List<(string Period, int Count)>> GetGrowthChartAsync(int months, CancellationToken cancellationToken = default);
}
