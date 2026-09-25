using System;

namespace callbet.Application.Services;

public static class GeoCalculator
{
    private const double EarthRadiusKm = 6371.0;

    /// <summary>
    /// Calculates the great-circle distance between two points on the Earth's surface using the Haversine formula.
    /// </summary>
    public static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2.0) * Math.Sin(dLat / 2.0) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2.0) * Math.Sin(dLon / 2.0);

        var c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));
        return Math.Round(EarthRadiusKm * c, 2);
    }

    /// <summary>
    /// Nullable decimal overload for entity coordinates.
    /// </summary>
    public static double? CalculateDistanceKm(decimal? lat1, decimal? lon1, decimal? lat2, decimal? lon2)
    {
        if (!lat1.HasValue || !lon1.HasValue || !lat2.HasValue || !lon2.HasValue)
        {
            return null;
        }

        return CalculateDistanceKm(
            (double)lat1.Value,
            (double)lon1.Value,
            (double)lat2.Value,
            (double)lon2.Value
        );
    }

    private static double ToRadians(double degrees) => degrees * (Math.PI / 180.0);
}
