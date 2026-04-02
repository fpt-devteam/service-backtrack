using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.SearchPosts;

public sealed class SearchPostsHandler(IPostRepository postRepository, IEmbeddingService embeddingService)
    : IRequestHandler<SearchPostsCommand, PagedResult<SearchPostResult>>
{
    public async Task<PagedResult<SearchPostResult>> Handle(SearchPostsCommand command, CancellationToken cancellationToken)
    {
        PostType? postType = null;
        if (!string.IsNullOrWhiteSpace(command.Filters?.PostType))
        {
            if (!Enum.TryParse<PostType>(command.Filters.PostType, ignoreCase: true, out var parsed))
                throw new ValidationException(PostErrors.InvalidPostType);
            postType = parsed;
        }

        var pagedQuery = PagedQuery.FromPage(command.Page, command.PageSize);
        var location = command.Filters?.Location;
        var radiusInKm = command.Filters?.RadiusInKm;
        var organizationId = command.Filters?.OrganizationId;

        if (command.Mode == SearchMode.Semantic)
            return await HandleSemanticAsync(command, postType, pagedQuery, location, radiusInKm, organizationId, cancellationToken);

        return await HandleKeywordAsync(command, postType, pagedQuery, location, radiusInKm, organizationId, cancellationToken);
    }

    private async Task<PagedResult<SearchPostResult>> HandleKeywordAsync(
        SearchPostsCommand command,
        PostType? postType,
        PagedQuery pagedQuery,
        GeoPoint? location,
        double? radiusInKm,
        Guid? organizationId,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await postRepository.GetPagedAsync(
            pagedQuery: pagedQuery,
            searchTerm: command.Query,
            postType: postType,
            organizationId: organizationId,
            location: location,
            radiusInKm: radiusInKm,
            cancellationToken: cancellationToken);

        var results = items.Select(item =>
        {
            var (post, distanceMeters) = item;
            return new SearchPostResult
            {
                Id = post.Id,
                Author = post.Author?.ToPostAuthorResult(),
                Organization = post.Organization?.ToOrganizationOnPost(),
                PostType = post.PostType,
                Item = post.Item,
                ImageUrls = post.ImageUrls,
                Location = post.Location,
                ExternalPlaceId = post.ExternalPlaceId,
                DisplayAddress = post.DisplayAddress,
                EventTime = post.EventTime,
                CreatedAt = post.CreatedAt,
                DistanceInMeters = distanceMeters
            };
        }).ToList();

        return new PagedResult<SearchPostResult>(totalCount, results);
    }

    private async Task<PagedResult<SearchPostResult>> HandleSemanticAsync(
        SearchPostsCommand command,
        PostType? postType,
        PagedQuery pagedQuery,
        GeoPoint? location,
        double? radiusInKm,
        Guid? organizationId,
        CancellationToken cancellationToken)
    {
        var searchText = command.Query ?? string.Empty;
        var enhancedQuery = $@"I am searching for: {searchText}
Item description: {searchText}

Looking for information about {searchText.ToLower()}.";

        var queryEmbedding = await embeddingService.GenerateMultimodalEmbeddingAsync(enhancedQuery, null, null, cancellationToken);

        var (items, totalCount) = await postRepository.SearchBySemanticAsync(
            queryEmbedding: queryEmbedding,
            pagedQuery: pagedQuery,
            postType: postType,
            location: location,
            radiusInKm: radiusInKm,
            organizationId: organizationId,
            cancellationToken: cancellationToken);

        var results = items.Select(item =>
        {
            var (post, similarityScore, distanceMeters) = item;
            return new SearchPostResult
            {
                Id = post.Id,
                Author = post.Author?.ToPostAuthorResult(),
                Organization = post.Organization?.ToOrganizationOnPost(),
                PostType = post.PostType,
                Item = post.Item,
                ImageUrls = post.ImageUrls,
                Location = post.Location,
                ExternalPlaceId = post.ExternalPlaceId,
                DisplayAddress = post.DisplayAddress,
                EventTime = post.EventTime,
                CreatedAt = post.CreatedAt,
                Score = similarityScore,
                DistanceInMeters = distanceMeters
            };
        }).ToList();

        return new PagedResult<SearchPostResult>(totalCount, results);
    }
}
