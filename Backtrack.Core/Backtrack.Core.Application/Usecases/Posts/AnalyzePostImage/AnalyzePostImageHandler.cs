using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.AnalyzePostImage;

public sealed class AnalyzePostImageHandler(
    IImageAnalysisService imageAnalysisService,
    IOcrService ocrService,
    IImageFetcher imageFetcher) : IRequestHandler<AnalyzePostImageCommand, ImageAnalysisResult>
{
    public async Task<ImageAnalysisResult> Handle(AnalyzePostImageCommand command, CancellationToken cancellationToken)
    {
        var fetchTasks = command.ImageUrls
            .Select(url => imageFetcher.FetchAsync(url, cancellationToken));

        var fetchResults = await Task.WhenAll(fetchTasks);

        var fetched = fetchResults.FirstOrDefault(r => r is not null)
            ?? throw new ValidationException(PostErrors.ImageFetchFailed);

        Enum.TryParse<ItemCategory>(command.Category, ignoreCase: true, out var category);

        return category switch
        {
            ItemCategory.PersonalBelongings => new ImageAnalysisResult
            {
                Category = command.Category,
                PersonalBelonging = await imageAnalysisService.AnalyzePersonalBelongingAsync(fetched.Base64, fetched.MimeType, cancellationToken)
            },
            ItemCategory.Electronics => new ImageAnalysisResult
            {
                Category = command.Category,
                Electronic = await imageAnalysisService.AnalyzeElectronicAsync(fetched.Base64, fetched.MimeType, cancellationToken)
            },
            ItemCategory.Cards => new ImageAnalysisResult
            {
                Category = command.Category,
                Card = await ocrService.ExtractCardTextAsync(fetched.Base64, fetched.MimeType, cancellationToken)
            },
            ItemCategory.Others => new ImageAnalysisResult
            {
                Category = command.Category,
                Other = await imageAnalysisService.AnalyzeOtherAsync(fetched.Base64, fetched.MimeType, cancellationToken)
            },
            _ => throw new ValidationException(PostErrors.InvalidCategory)
        };
    }
}
