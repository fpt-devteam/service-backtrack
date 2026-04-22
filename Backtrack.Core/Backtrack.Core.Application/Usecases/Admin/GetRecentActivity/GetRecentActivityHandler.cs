using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.PostExplorations;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Admin.GetRecentActivity;

public sealed class GetRecentActivityHandler(
    IUserRepository userRepository,
    IPostRepository postRepository) : IRequestHandler<GetRecentActivityQuery, RecentActivityResult>
{
    public async Task<RecentActivityResult> Handle(GetRecentActivityQuery query, CancellationToken cancellationToken)
    {
        var caller = await userRepository.GetByIdAsync(query.AdminUserId);
        if (caller is null || caller.GlobalRole != UserGlobalRole.PlatformSuperAdmin)
            throw new ForbiddenException(AdminErrors.Forbidden);

        if (query.Limit < 1 || query.Limit > 50)
            throw new ValidationException(AdminErrors.InvalidLimitRange);

        PostStatus? statusFilter = null;
        if (query.Status is not null)
        {
            if (!Enum.TryParse<PostStatus>(query.Status, ignoreCase: true, out var parsed))
                throw new ValidationException(AdminErrors.InvalidPostStatus);
            statusFilter = parsed;
        }

        var filters    = statusFilter.HasValue ? new PostFilters { Status = statusFilter } : null;
        var pagedQuery = PagedQuery.FromPage(1, query.Limit);

        var total          = await postRepository.CountAsync(filters, cancellationToken);
        var (items, _)     = await postRepository.GetPagedAsync(pagedQuery, filters, cancellationToken);

        var now  = DateTimeOffset.UtcNow;
        var data = items.Select(post => new RecentActivityItemResult(
            PostId:     post.Id.ToString(),
            Title:      post.PostTitle,
            AuthorName: post.Author?.DisplayName ?? string.Empty,
            Initials:   BuildInitials(post.Author?.DisplayName ?? string.Empty),
            Location:   post.DisplayAddress,
            Status:     post.Status.ToString(),
            CreatedAt:  post.CreatedAt,
            TimeAgo:    BuildTimeAgo(now, post.CreatedAt)
        )).ToList();

        return new RecentActivityResult(data, total);
    }

    private static string BuildInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;
        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[^1][0])}";
        var word = parts[0];
        return word.Length >= 2 ? word[..2].ToUpper() : word.ToUpper();
    }

    private static string BuildTimeAgo(DateTimeOffset now, DateTimeOffset createdAt)
    {
        var diff = now - createdAt;
        if (diff.TotalSeconds < 60) return $"{(int)diff.TotalSeconds}s ago";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
        if (diff.TotalHours   < 24) return $"{(int)diff.TotalHours}h ago";
        return $"{(int)diff.TotalDays}d ago";
    }
}
