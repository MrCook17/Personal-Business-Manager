namespace PersonalBusinessManager.Core.Domain.Calculations;

public sealed record InvoiceSettlement(
    decimal AmountPaid,
    decimal OutstandingAmount,
    decimal OverpaymentAmount,
    string StatusCode);

public static class InvoiceSettlementCalculator
{
    public const string CreditedStatus = "credited";
    public const string PaidStatus = "paid";
    public const string PartPaidStatus = "part_paid";
    public const string SentStatus = "sent";
    public const string FinalisedStatus = "finalised";

    public static InvoiceSettlement Calculate(
        decimal grossTotal,
        decimal validPayments,
        decimal finalisedCreditTotal,
        bool wasSent)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(grossTotal);
        ArgumentOutOfRangeException.ThrowIfNegative(validPayments);
        ArgumentOutOfRangeException.ThrowIfNegative(
            finalisedCreditTotal);

        decimal roundedGross = MoneyRounding.Round(grossTotal);
        decimal roundedPayments = MoneyRounding.Round(
            validPayments);
        decimal roundedCredits = MoneyRounding.Round(
            finalisedCreditTotal);
        decimal remainingAfterCredit = Math.Max(
            roundedGross - roundedCredits,
            0m);
        decimal outstanding = Math.Max(
            remainingAfterCredit - roundedPayments,
            0m);
        decimal overpayment = Math.Max(
            roundedPayments - remainingAfterCredit,
            0m);

        string statusCode;

        if (roundedGross > 0m
            && roundedCredits >= roundedGross)
        {
            statusCode = CreditedStatus;
        }
        else if (roundedPayments > 0m
            && outstanding == 0m)
        {
            statusCode = PaidStatus;
        }
        else if (roundedPayments > 0m)
        {
            statusCode = PartPaidStatus;
        }
        else
        {
            statusCode = wasSent
                ? SentStatus
                : FinalisedStatus;
        }

        return new InvoiceSettlement(
            roundedPayments,
            outstanding,
            overpayment,
            statusCode);
    }
}
