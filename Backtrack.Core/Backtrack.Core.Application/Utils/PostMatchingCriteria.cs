using Backtrack.Core.Domain.Constants;

namespace Backtrack.Core.Application.Utils;

/// <summary>
/// Central source of truth for all per-criteria score formulas and matching thresholds.
/// Visual and Description scores come from embedding similarities (0-1 → 0-100).
/// Location and TimeWindow scores are computed by deterministic formulas.
/// All four scores feed into the overall weighted MatchScore.
/// </summary>
public static class PostMatchingCriteria
{
    // ── Weighted MatchScore coefficients (must sum to 1.0) ──────────────────
    public const float TextSimilarityWeight  = 0.40f;
    public const float ImageSimilarityWeight = 0.35f;
    public const float LocationWeight        = 0.25f;

    // ── Geographic cap ────────────────────────────────────────────────────────
    public const double MaxDistanceMeters = 20_000; // 20 km

    // ── Time window cap ───────────────────────────────────────────────────────
    public const double MaxTimeWindowDays = 30.0;

    // ── Candidate filter ──────────────────────────────────────────────────────
    public const float MinMatchScore = 0.40f;

    // ── MatchingLevel thresholds (applied to final MatchScore 0-1) ───────────
    public const float VeryHighThreshold = 0.80f;
    public const float HighThreshold     = 0.65f;
    public const float MediumThreshold   = 0.45f;

    // ─────────────────────────────────────────────────────────────────────────
    // MatchingLevel
    // ─────────────────────────────────────────────────────────────────────────

    public static MatchingLevel ComputeMatchingLevel(float matchScore) => matchScore switch
    {
        >= VeryHighThreshold => MatchingLevel.VeryHigh,
        >= HighThreshold     => MatchingLevel.High,
        >= MediumThreshold   => MatchingLevel.Medium,
        _                    => MatchingLevel.Low
    };

    // ─────────────────────────────────────────────────────────────────────────
    // Overall weighted MatchScore (0-1), used for ranking and MatchingLevel
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Weighted combination of text similarity, image similarity, and a
    /// location proximity ratio derived from distanceMeters.
    /// Both posts are required to have an image embedding (ImageSimilarity is always present).
    /// </summary>
    public static float ComputeWeightedScore(
        double textSimilarity,
        double imageSimilarity,
        double distanceMeters)
    {
        float locRatio = 1.0f - (float)(Math.Min(distanceMeters, MaxDistanceMeters) / MaxDistanceMeters);

        return (float)textSimilarity * TextSimilarityWeight
             + (float)imageSimilarity * ImageSimilarityWeight
             + locRatio * LocationWeight;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Per-criteria 0-100 display scores
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Description score — directly from text embedding cosine similarity.</summary>
    public static int ComputeDescriptionScore(double textSimilarity)
        => Clamp100(textSimilarity * 100.0);

    /// <summary>Visual score — directly from image embedding cosine similarity.</summary>
    public static int ComputeVisualScore(double imageSimilarity)
        => Clamp100(imageSimilarity * 100.0);

    /// <summary>
    /// Location score — square-root decay so close distances score very high.
    /// 0 m → 100 | ~1 km → 78 | ~5 km → 50 | 20 km → 0
    /// </summary>
    public static int ComputeLocationScore(double distanceMeters)
    {
        if (distanceMeters >= MaxDistanceMeters) return 0;
        var ratio = distanceMeters / MaxDistanceMeters;
        return Clamp100((1.0 - Math.Sqrt(ratio)) * 100.0);
    }

    /// <summary>
    /// Time window score — square-root decay on absolute day difference.
    /// Same day → 100 | 1 day → 82 | 7 days → 52 | 30 days → 0.
    /// A found-before-lost gap of more than 24 h gets a hard penalty (score capped at 20).
    /// </summary>
    public static int ComputeTimeWindowScore(DateTimeOffset lostTime, DateTimeOffset foundTime)
    {
        var diff = foundTime - lostTime;

        // Found significantly before the item was reported lost — suspicious
        if (diff.TotalHours < -24)
            return 20;

        var diffDays = Math.Abs(diff.TotalDays);
        if (diffDays >= MaxTimeWindowDays) return 0;

        return Clamp100((1.0 - Math.Sqrt(diffDays / MaxTimeWindowDays)) * 100.0);
    }

    private static int Clamp100(double value)
        => (int)Math.Round(Math.Clamp(value, 0, 100));
}
