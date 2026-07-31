namespace PersonalBusinessManager.Core.Domain.Calculations;

public enum AccountClassification
{
    Asset,
    Liability,
}

public sealed record FinancialAccountBalance(
    AccountClassification Classification,
    decimal CurrentBalance);

public sealed record NetWorthResult(
    decimal TotalAssets,
    decimal TotalLiabilities,
    decimal NetWorth);

public static class NetWorthCalculator
{
    public static NetWorthResult Calculate(
        IEnumerable<FinancialAccountBalance> accounts)
    {
        ArgumentNullException.ThrowIfNull(accounts);

        FinancialAccountBalance[] balances = [.. accounts];
        decimal totalAssets = MoneyRounding.Round(
            balances
                .Where(account =>
                    account.Classification
                    == AccountClassification.Asset)
                .Sum(account => account.CurrentBalance));
        decimal totalLiabilities = MoneyRounding.Round(
            balances
                .Where(account =>
                    account.Classification
                    == AccountClassification.Liability)
                .Sum(account => account.CurrentBalance));

        return new NetWorthResult(
            totalAssets,
            totalLiabilities,
            MoneyRounding.Round(
                totalAssets - totalLiabilities));
    }
}
