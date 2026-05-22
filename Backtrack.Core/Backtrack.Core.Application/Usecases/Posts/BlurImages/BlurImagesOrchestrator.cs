using Backtrack.Core.Application.Interfaces.Helpers;
using Backtrack.Core.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Application.Usecases.Posts.BlurImages;

public sealed class BlurImagesOrchestrator(
    IPostRepository postRepository,
    IImageBlurService imageBlurService,
    ILogger<BlurImagesOrchestrator> logger)
{
    public async Task RunAsync(Guid postId)
    {
        var post = await postRepository.GetByIdAsync(postId, isTrack: true);
        if (post is null || post.ImageUrls.Count == 0)
        {
            logger.LogInformation("BlurImagesOrchestrator — post {PostId} not found or has no images.", postId);
            return;
        }

        var blurUrls = await imageBlurService.GenerateAndUploadAsync(
            post.ImageUrls,
            $"posts/{postId}/blur");

        post.BlurImageUrls = blurUrls;
        postRepository.Update(post);
        await postRepository.SaveChangesAsync();

        logger.LogInformation("BlurImagesOrchestrator — generated {Count} blur images for post {PostId}.", blurUrls.Count, postId);
    }
}
