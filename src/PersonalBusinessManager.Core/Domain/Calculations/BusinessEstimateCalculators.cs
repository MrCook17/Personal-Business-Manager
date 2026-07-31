namespace PersonalBusinessManager.Core.Domain.Calculations;

public static class ProfitEstimateCalculator
{
    public static decimal Calculate(
        decimal revenue,
        decimal expenses)
    {
        return MoneyRounding.Round(revenue - expenses);
    }
}

public static class TaxReserveCalculator
{
    public static decimal Calculate(
        decimal estimatedProfit,
        decimal percentage)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(percentage);

        if (percentage > 100m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(percentage),
                "Tax-reserve percentage cannot exceed 100 percent.");
        }

        return MoneyRounding.Round(
            Math.Max(estimatedProfit, 0m)
            * percentage
            / 100m);
    }
}
