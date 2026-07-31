namespace PersonalBusinessManager.Core.Domain.Calculations;

public enum TimeRoundingRule
{
    None,
    Nearest5,
    Nearest6,
    Nearest10,
    Nearest15,
    Up5,
    Up6,
    Up10,
    Up15,
}

public static class DurationCalculator
{
    public static long CalculateSeconds(
        DateTimeOffset startUtc,
        DateTimeOffset endUtc)
    {
        TimeSpan duration = endUtc - startUtc;

        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endUtc),
                "End time must be later than start time.");
        }

        if (duration.Ticks % TimeSpan.TicksPerSecond != 0)
        {
            throw new ArgumentException(
                "Duration must resolve to a whole number of seconds.",
                nameof(endUtc));
        }

        return duration.Ticks / TimeSpan.TicksPerSecond;
    }
}

public static class TimeRoundingCalculator
{
    public static long RoundSeconds(
        long rawSeconds,
        TimeRoundingRule rule)
    {
        if (rawSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(rawSeconds),
                "Duration must be positive.");
        }

        if (rule == TimeRoundingRule.None)
        {
            return rawSeconds;
        }

        int intervalSeconds = GetIntervalSeconds(rule);
        long quotient = Math.DivRem(
            rawSeconds,
            intervalSeconds,
            out long remainder);

        bool roundsUp = rule is
            TimeRoundingRule.Up5
            or TimeRoundingRule.Up6
            or TimeRoundingRule.Up10
            or TimeRoundingRule.Up15
            ? remainder > 0
            : remainder * 2 >= intervalSeconds;

        return checked(
            (quotient + (roundsUp ? 1 : 0))
            * intervalSeconds);
    }

    private static int GetIntervalSeconds(
        TimeRoundingRule rule)
    {
        return rule switch
        {
            TimeRoundingRule.Nearest5
                or TimeRoundingRule.Up5 => 5 * 60,
            TimeRoundingRule.Nearest6
                or TimeRoundingRule.Up6 => 6 * 60,
            TimeRoundingRule.Nearest10
                or TimeRoundingRule.Up10 => 10 * 60,
            TimeRoundingRule.Nearest15
                or TimeRoundingRule.Up15 => 15 * 60,
            _ => throw new ArgumentOutOfRangeException(
                nameof(rule)),
        };
    }
}
