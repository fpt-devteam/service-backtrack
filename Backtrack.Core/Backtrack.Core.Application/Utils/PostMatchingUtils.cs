using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Application.Utils;

public static class PostMatchingUtils
{
    public const float VeryHighThreshold = 0.90f;
    public const float HighThreshold = 0.80f;
    public const float MediumThreshold = 0.75f;


    public static MatchingLevel ComputeMatchingLevel(float matchScore) => matchScore switch
    {
        >= VeryHighThreshold => MatchingLevel.VeryHigh,
        >= HighThreshold => MatchingLevel.High,
        >= MediumThreshold => MatchingLevel.Medium,
        _ => throw new ArgumentException($"Match score {matchScore} is below the minimum threshold of {MediumThreshold}")
    };

    public static int Clamp100(double value)
        => (int)Math.Round(Math.Clamp(value, 0, 100));
}
