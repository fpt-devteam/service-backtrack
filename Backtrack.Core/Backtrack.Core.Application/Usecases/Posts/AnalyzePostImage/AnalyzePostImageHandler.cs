using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.AnalyzePostImage;

public sealed class AnalyzePostImageHandler(
    IImageAnalysisService imageAnalysisService,
    IImageFetcher imageFetcher) : IRequestHandler<AnalyzePostImageCommand, PostItem>
{
    public async Task<PostItem> Handle(AnalyzePostImageCommand command, CancellationToken cancellationToken)
    {
        var fetchedImage = await imageFetcher.FetchAsync(command.ImageUrl, cancellationToken)
            ?? throw new ValidationException(PostErrors.ImageFetchFailed);

        return await imageAnalysisService.AnalyzeImageAsync(
            fetchedImage.Base64,
            fetchedImage.MimeType,
            cancellationToken);
    }
}
