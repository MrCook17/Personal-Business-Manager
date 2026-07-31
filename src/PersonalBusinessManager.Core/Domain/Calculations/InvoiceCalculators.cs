namespace PersonalBusinessManager.Core.Domain.Calculations;

public enum InvoiceDiscountType
{
    None,
    Percentage,
    FixedAmount,
}

public sealed record InvoiceLineAmounts(
    decimal BaseAmount,
    decimal DiscountAmount,
    decimal NetAmount,
    decimal VatAmount,
    decimal GrossAmount);

public sealed record InvoiceTotals(
    decimal NetTotal,
    decimal VatTotal,
    decimal GrossTotal,
    decimal DiscountTotal);

public static class InvoiceLineCalculator
{
    public static InvoiceLineAmounts Calculate(
        decimal quantity,
        decimal unitRate,
        InvoiceDiscountType discountType,
        decimal discountValue,
        decimal vatRate,
        bool pricesIncludeVat = false)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);
        ArgumentOutOfRangeException.ThrowIfNegative(unitRate);
        ArgumentOutOfRangeException.ThrowIfNegative(discountValue);
        ArgumentOutOfRangeException.ThrowIfNegative(vatRate);

        if (vatRate > 100m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(vatRate),
                "VAT rate cannot exceed 100 percent.");
        }

        decimal baseAmount = MoneyRounding.Round(
            quantity * unitRate);
        decimal discountAmount = CalculateDiscount(
            baseAmount,
            discountType,
            discountValue);

        if (pricesIncludeVat)
        {
            decimal grossAmount = baseAmount - discountAmount;
            decimal netAmount = vatRate == 0m
                ? grossAmount
                : MoneyRounding.Round(
                    grossAmount / (1m + vatRate / 100m));
            decimal vatAmount = grossAmount - netAmount;

            return new InvoiceLineAmounts(
                baseAmount,
                discountAmount,
                netAmount,
                vatAmount,
                grossAmount);
        }

        decimal exclusiveNetAmount =
            baseAmount - discountAmount;
        decimal exclusiveVatAmount = MoneyRounding.Round(
            exclusiveNetAmount * vatRate / 100m);

        return new InvoiceLineAmounts(
            baseAmount,
            discountAmount,
            exclusiveNetAmount,
            exclusiveVatAmount,
            exclusiveNetAmount + exclusiveVatAmount);
    }

    private static decimal CalculateDiscount(
        decimal baseAmount,
        InvoiceDiscountType discountType,
        decimal discountValue)
    {
        decimal discountAmount = discountType switch
        {
            InvoiceDiscountType.None when discountValue == 0m =>
                0m,
            InvoiceDiscountType.None =>
                throw new ArgumentException(
                    "A line without a discount must have a zero discount value.",
                    nameof(discountValue)),
            InvoiceDiscountType.Percentage
                when discountValue <= 100m =>
                MoneyRounding.Round(
                    baseAmount * discountValue / 100m),
            InvoiceDiscountType.Percentage =>
                throw new ArgumentOutOfRangeException(
                    nameof(discountValue),
                    "Percentage discount cannot exceed 100 percent."),
            InvoiceDiscountType.FixedAmount =>
                MoneyRounding.Round(discountValue),
            _ => throw new ArgumentOutOfRangeException(
                nameof(discountType)),
        };

        if (discountAmount > baseAmount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(discountValue),
                "Fixed discount cannot exceed the line base amount.");
        }

        return discountAmount;
    }
}

public static class InvoiceTotalCalculator
{
    public static InvoiceTotals Calculate(
        IEnumerable<InvoiceLineAmounts> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        InvoiceLineAmounts[] storedLines = [.. lines];

        return new InvoiceTotals(
            storedLines.Sum(line => line.NetAmount),
            storedLines.Sum(line => line.VatAmount),
            storedLines.Sum(line => line.GrossAmount),
            storedLines.Sum(line => line.DiscountAmount));
    }
}
