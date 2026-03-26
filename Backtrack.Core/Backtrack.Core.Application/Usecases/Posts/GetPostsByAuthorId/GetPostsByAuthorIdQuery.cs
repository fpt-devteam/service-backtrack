using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.GetPostsByAuthorId;

public sealed record GetPostsByAuthorIdQuery(string AuthorId) : IRequest<List<PostResult>>;
