namespace Golbet.Helpers;

public static class DateTimeExtensions
{
    public static DateTime ToColombiaTime(this DateTime utcDateTime)
    {
        var colombiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");

        return TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc),
            colombiaTimeZone);
    }
}