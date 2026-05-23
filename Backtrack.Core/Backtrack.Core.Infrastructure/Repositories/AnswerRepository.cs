using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data;

namespace Backtrack.Core.Infrastructure.Repositories;

/// <summary>
/// EF Core repository implementation for Answer entities.
/// </summary>
public sealed class AnswerRepository(ApplicationDbContext context)
    : CrudRepositoryBase<Answer, Guid>(context), IAnswerRepository
{
}
