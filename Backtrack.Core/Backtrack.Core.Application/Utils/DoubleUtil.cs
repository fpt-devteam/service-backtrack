namespace Backtrack.Core.Application.Utils;

public static class DoubleUtil
{
    private const double DefaultEpsilon = 1e-6;

    /// <summary>
    /// Checks if two doubles are approximately equal within a small epsilon.
    /// This is useful for geographic coordinates where small precision differences 
    /// from different clients or serialization should not trigger recalculations.
    /// </summary>
    public static bool AreApproximatelyEqual(double a, double b, double epsilon = DefaultEpsilon)
    {
        return Math.Abs(a - b) < epsilon;
    }

    /// <summary>
    /// Checks if two doubles are not approximately equal.
    /// </summary>
    public static bool AreNotApproximatelyEqual(double a, double b, double epsilon = DefaultEpsilon)
    {
        return !AreApproximatelyEqual(a, b, epsilon);
    }
}
