using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Exceptions.Errors;
using Backtrack.Core.Application.Interfaces.AI;
using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Domain.Constants;
using MediatR;

namespace Backtrack.Core.Application.Usecases.Posts.AnalyzePostImage;

public sealed class AnalyzePostImageHandler(
    IImageAnalysisService imageAnalysisService,
    IOcrService ocrService,
    IImageFetcher imageFetcher,
    ISubcategoryRepository subcategoryRepository) : IRequestHandler<AnalyzePostImageCommand, ImageAnalysisResult>
{
    public async Task<ImageAnalysisResult> Handle(AnalyzePostImageCommand command, CancellationToken cancellationToken)
    {
        var subcategory = await subcategoryRepository.GetByCodeAsync(command.SubcategoryCode, cancellationToken)
            ?? throw new NotFoundException(PostErrors.SubcategoryNotFound);

        var fetchTasks = command.ImageUrls.Select(url => imageFetcher.FetchAsync(url, cancellationToken));
        var fetchResults = await Task.WhenAll(fetchTasks);

        var fetched = fetchResults.Where(r => r is not null).Cast<FetchedImage>().ToList();
        if (fetched.Count == 0)
            throw new ValidationException(PostErrors.ImageFetchFailed);

        var consistency = await imageAnalysisService.VerifyItemConsistencyAsync(fetched, subcategory.Name, cancellationToken);

        if (!consistency.MatchesSubcategory)
            throw new ValidationException(PostErrors.ImageDoesNotMatchSubcategory, BuildDetails(consistency));

        var primary = fetched[0];

        return subcategory.Category switch
        {
            ItemCategory.PersonalBelongings => new ImageAnalysisResult
            {
                Category          = subcategory.Category.ToString(),
                PersonalBelonging = await imageAnalysisService.AnalyzePersonalBelongingAsync(primary.Base64, primary.MimeType, cancellationToken)
            },
            ItemCategory.Electronics => new ImageAnalysisResult
            {
                Category   = subcategory.Category.ToString(),
                Electronic = await imageAnalysisService.AnalyzeElectronicAsync(primary.Base64, primary.MimeType, cancellationToken)
            },
            ItemCategory.Cards => new ImageAnalysisResult
            {
                Category = subcategory.Category.ToString(),
                Card     = await ocrService.ExtractCardTextAsync(primary.Base64, primary.MimeType, cancellationToken)
            },
            ItemCategory.Others => new ImageAnalysisResult
            {
                Category = subcategory.Category.ToString(),
                Other    = await imageAnalysisService.AnalyzeOtherAsync(primary.Base64, primary.MimeType, cancellationToken)
            },
            _ => throw new ValidationException(PostErrors.InvalidCategory)
        };
    }

    private static IReadOnlyDictionary<string, string[]>? BuildDetails(ItemConsistencyResult consistency)
    {
        var dict = new Dictionary<string, string[]>();
        if (consistency.Reason is not null)
            dict["reason"] = [consistency.Reason];
        if (consistency.SuggestedSubcategoryCode is not null)
            dict["suggestedSubcategoryCode"] = [consistency.SuggestedSubcategoryCode];
        return dict.Count > 0 ? dict : null;
    }
}
