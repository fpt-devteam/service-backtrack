namespace Backtrack.Core.Application.Utils.PostSimilarity;

public static class SimilarityScoreCalculator
{
    public static double CalculateTotalSimilarity(double descriptionSimilarity, double locationSimilarity)
    {
        return (descriptionSimilarity * SimilarityCriteria.DescriptionWeight)
                            + (locationSimilarity * SimilarityCriteria.LocationWeight);
    }

    public static double CalculateLocationSimilarity(double distanceMeters)
    {
        if (distanceMeters <= 0) return 1.0;
        if (distanceMeters >= SimilarityCriteria.MaxDistanceMeters) return 0.0;

        return 1.0 - (distanceMeters / SimilarityCriteria.MaxDistanceMeters);
    }
}