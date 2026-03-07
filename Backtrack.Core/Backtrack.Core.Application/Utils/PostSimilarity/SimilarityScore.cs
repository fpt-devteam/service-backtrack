namespace Backtrack.Core.Application.Utils.PostSimilarity;

public record SimilarityScore(
    double DescriptionSimilarity,
    double LocationSimilarity,
    double TotalSimilarity,
    double DistanceMeters
);