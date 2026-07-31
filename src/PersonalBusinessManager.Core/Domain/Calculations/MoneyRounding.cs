namespace PersonalBusinessManager.Core.Domain.Calculations;

public static class MoneyRounding
{
    public static decimal Round(decimal value)
    {
        return decimal.Round(
            value,
            2,
            MidpointRounding.AwayFromZero);
    }
}
