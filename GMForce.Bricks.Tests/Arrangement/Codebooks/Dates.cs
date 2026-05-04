namespace GMForce.Bricks.Tests.Arrangement.Codebooks;

internal static class Dates
{
    internal const string ShortDotFormat = "15.1.2024.";
    internal const string SlashFormat = "01/15/2024";
    internal const string DotFormat = "15.01.2024";
    internal const string UnsupportedFormat = "2024-01-15";
    internal static readonly DateOnly ExpectedDate = new(2024, 1, 15);
}
