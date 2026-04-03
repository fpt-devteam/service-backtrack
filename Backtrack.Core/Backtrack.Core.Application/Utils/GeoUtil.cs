using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Application.Utils;

public static class GeoUtil
{
    private const double EarthRadiusMeters = 6_371_000;

    /// <summary>
    /// Calculates the great-circle distance in meters between two geographic points
    /// using the Haversine formula.
    /// </summary>
    public static double Haversine(GeoPoint from, GeoPoint to)
        => Haversine(from.Latitude, from.Longitude, to.Latitude, to.Longitude);

    public static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        var φ1 = lat1 * Math.PI / 180;
        var φ2 = lat2 * Math.PI / 180;
        var Δφ = (lat2 - lat1) * Math.PI / 180;
        var Δλ = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                Math.Cos(φ1) * Math.Cos(φ2) *
                Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
        return EarthRadiusMeters * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
