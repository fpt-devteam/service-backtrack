namespace Backtrack.Core.Application.Utils.PostSimilarity;

public static class SimilarityCriteria
{
    public const double DescriptionWeight = 0.8;
    public const double LocationWeight = 0.2;
    public const double DescriptionSimilarityThreshold = 0.5;
    public const double MaxDistanceMeters = 20000;
    public const double TotalSimilarityThreshold = 0.7;
}