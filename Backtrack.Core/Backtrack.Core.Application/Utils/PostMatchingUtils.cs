using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Application.Utils;

public static class PostMatchingUtils
{
    public static MatchingLevel ComputeMatchingLevel(float matchScore) => matchScore switch
    {
        >= PostSimilarityThresholds.VerySimilarityHighThreshold => MatchingLevel.VeryHigh,
        >= PostSimilarityThresholds.HighSimilarityThreshold => MatchingLevel.High,
        >= PostSimilarityThresholds.MediumSimilarityThreshold => MatchingLevel.Medium,
        _ => throw new ArgumentException($"Match score {matchScore} is below the minimum threshold of {PostSimilarityThresholds.MediumSimilarityThreshold}")
    };

    public static int Clamp100(double value)
        => (int)Math.Round(Math.Clamp(value, 0, 100));
}
