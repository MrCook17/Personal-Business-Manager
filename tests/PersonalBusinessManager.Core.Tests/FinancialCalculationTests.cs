using System.Globalization;
using PersonalBusinessManager.Core.Domain.Calculations;

namespace PersonalBusinessManager.Core.Tests;

public sealed class FinancialCalculationTests
{
    [Theory]
    [InlineData("1.004", "1.00")]
    [InlineData("1.005", "1.01")]
    [InlineData("1.006", "1.01")]
    [InlineData("-1.004", "-1.00")]
    [InlineData("-1.005", "-1.01")]
    [InlineData("-1.006", "-1.01")]
    [InlineData("2.675", "2.68")]
    [InlineData("10.995", "11.00")]
    public void RoundUsesTwoDecimalPlacesAwayFromZero(
        string input,
        string expected)
    {
        Assert.Equal(
            decimal.Parse(
                expected,
                CultureInfo.InvariantCulture),
            MoneyRounding.Round(decimal.Parse(
                input,
                CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void PercentageDiscountRoundsBeforeVat()
    {
        InvoiceLineAmounts result =
            InvoiceLineCalculator.Calculate(
                10m,
                12.99m,
                InvoiceDiscountType.Percentage,
                15m,
                20m);

        Assert.Equal(19.49m, result.DiscountAmount);
        Assert.Equal(110.41m, result.NetAmount);
        Assert.Equal(22.08m, result.VatAmount);
        Assert.Equal(132.49m, result.GrossAmount);
    }

    [Fact]
    public void FixedDiscountIsAppliedBeforeVat()
    {
        InvoiceLineAmounts result =
            InvoiceLineCalculator.Calculate(
                4m,
                25m,
                InvoiceDiscountType.FixedAmount,
                7.5m,
                20m);

        Assert.Equal(7.5m, result.DiscountAmount);
        Assert.Equal(92.5m, result.NetAmount);
        Assert.Equal(18.5m, result.VatAmount);
        Assert.Equal(111m, result.GrossAmount);
    }

    [Fact]
    public void VatIsCalculatedFromTheDiscountedNetAmount()
    {
        InvoiceLineAmounts result =
            InvoiceLineCalculator.Calculate(
                2m,
                75m,
                InvoiceDiscountType.Percentage,
                10m,
                20m);

        Assert.Equal(135m, result.NetAmount);
        Assert.Equal(27m, result.VatAmount);
        Assert.Equal(162m, result.GrossAmount);
    }

    [Fact]
    public void VatInclusiveLineDerivesNetAndVatFromStoredGross()
    {
        InvoiceLineAmounts result =
            InvoiceLineCalculator.Calculate(
                3m,
                39.99m,
                InvoiceDiscountType.None,
                0m,
                20m,
                pricesIncludeVat: true);

        Assert.Equal(99.98m, result.NetAmount);
        Assert.Equal(19.99m, result.VatAmount);
        Assert.Equal(119.97m, result.GrossAmount);
    }

    [Fact]
    public void InvoiceTotalsSumStoredRoundedLineValues()
    {
        InvoiceLineAmounts[] lines =
        [
            new(70m, 0m, 70m, 14m, 84m),
            new(37.5m, 3.75m, 33.75m, 6.75m, 40.5m),
            new(20m, 0m, 20m, 0m, 20m),
        ];

        InvoiceTotals result =
            InvoiceTotalCalculator.Calculate(lines);

        Assert.Equal(123.75m, result.NetTotal);
        Assert.Equal(20.75m, result.VatTotal);
        Assert.Equal(144.5m, result.GrossTotal);
        Assert.Equal(3.75m, result.DiscountTotal);
    }

    [Fact]
    public void PercentageDiscountAboveOneHundredIsRejected()
    {
        ArgumentOutOfRangeException exception = Assert.Throws<
            ArgumentOutOfRangeException>(() =>
                InvoiceLineCalculator.Calculate(
                    1m,
                    50m,
                    InvoiceDiscountType.Percentage,
                    100.0001m,
                    20m));

        Assert.Equal("discountValue", exception.ParamName);
    }

    [Fact]
    public void FixedDiscountAboveLineBaseIsRejected()
    {
        ArgumentOutOfRangeException exception = Assert.Throws<
            ArgumentOutOfRangeException>(() =>
                InvoiceLineCalculator.Calculate(
                    1m,
                    50m,
                    InvoiceDiscountType.FixedAmount,
                    51m,
                    20m));

        Assert.Equal("discountValue", exception.ParamName);
    }

    [Fact]
    public void NoPaymentRetainsSentStatus()
    {
        InvoiceSettlement result =
            InvoiceSettlementCalculator.Calculate(
                600m,
                0m,
                0m,
                wasSent: true);

        Assert.Equal(600m, result.OutstandingAmount);
        Assert.Equal(
            InvoiceSettlementCalculator.SentStatus,
            result.StatusCode);
    }

    [Fact]
    public void PartPaymentSetsPartPaidStatus()
    {
        InvoiceSettlement result =
            InvoiceSettlementCalculator.Calculate(
                600m,
                200m,
                0m,
                wasSent: true);

        Assert.Equal(200m, result.AmountPaid);
        Assert.Equal(400m, result.OutstandingAmount);
        Assert.Equal(
            InvoiceSettlementCalculator.PartPaidStatus,
            result.StatusCode);
    }

    [Fact]
    public void FullPaymentSetsPaidStatus()
    {
        InvoiceSettlement result =
            InvoiceSettlementCalculator.Calculate(
                600m,
                600m,
                0m,
                wasSent: true);

        Assert.Equal(0m, result.OutstandingAmount);
        Assert.Equal(
            InvoiceSettlementCalculator.PaidStatus,
            result.StatusCode);
    }

    [Fact]
    public void FullCreditTakesStatusPrecedenceOverPayments()
    {
        InvoiceSettlement result =
            InvoiceSettlementCalculator.Calculate(
                600m,
                100m,
                600m,
                wasSent: true);

        Assert.Equal(0m, result.OutstandingAmount);
        Assert.Equal(100m, result.OverpaymentAmount);
        Assert.Equal(
            InvoiceSettlementCalculator.CreditedStatus,
            result.StatusCode);
    }

    [Fact]
    public void OverpaymentNeverCreatesNegativeOutstanding()
    {
        InvoiceSettlement result =
            InvoiceSettlementCalculator.Calculate(
                600m,
                625m,
                0m,
                wasSent: true);

        Assert.Equal(0m, result.OutstandingAmount);
        Assert.Equal(25m, result.OverpaymentAmount);
    }

    [Fact]
    public void DurationUsesTheExactWholeSecondDifference()
    {
        DateTimeOffset start = new(
            2026,
            7,
            29,
            9,
            12,
            14,
            TimeSpan.Zero);
        DateTimeOffset end = new(
            2026,
            7,
            29,
            10,
            14,
            44,
            TimeSpan.Zero);

        Assert.Equal(
            3_750,
            DurationCalculator.CalculateSeconds(start, end));
    }

    [Fact]
    public void DurationRejectsAnEndThatIsNotAfterTheStart()
    {
        DateTimeOffset timestamp = DateTimeOffset.UtcNow;

        ArgumentOutOfRangeException exception = Assert.Throws<
            ArgumentOutOfRangeException>(() =>
                DurationCalculator.CalculateSeconds(
                    timestamp,
                    timestamp));

        Assert.Equal("endUtc", exception.ParamName);
    }

    [Theory]
    [InlineData(TimeRoundingRule.None, 3_750)]
    [InlineData(TimeRoundingRule.Nearest5, 3_900)]
    [InlineData(TimeRoundingRule.Nearest6, 3_600)]
    [InlineData(TimeRoundingRule.Nearest10, 3_600)]
    [InlineData(TimeRoundingRule.Nearest15, 3_600)]
    [InlineData(TimeRoundingRule.Up5, 3_900)]
    [InlineData(TimeRoundingRule.Up6, 3_960)]
    [InlineData(TimeRoundingRule.Up10, 4_200)]
    [InlineData(TimeRoundingRule.Up15, 4_500)]
    public void TimeRoundingMatchesEveryApprovedRule(
        TimeRoundingRule rule,
        long expectedSeconds)
    {
        Assert.Equal(
            expectedSeconds,
            TimeRoundingCalculator.RoundSeconds(
                3_750,
                rule));
    }

    [Fact]
    public void UpRoundingLeavesAnExactBoundaryUnchanged()
    {
        Assert.Equal(
            3_600,
            TimeRoundingCalculator.RoundSeconds(
                3_600,
                TimeRoundingRule.Up15));
    }

    [Fact]
    public void MixedSignedAccountsCalculateApprovedNetWorth()
    {
        FinancialAccountBalance[] accounts =
        [
            new(AccountClassification.Asset, 2_500m),
            new(AccountClassification.Asset, 10_000m),
            new(AccountClassification.Asset, -200m),
            new(AccountClassification.Liability, 750m),
            new(AccountClassification.Liability, 2_000m),
            new(AccountClassification.Liability, -50m),
        ];

        NetWorthResult result =
            NetWorthCalculator.Calculate(accounts);

        Assert.Equal(12_300m, result.TotalAssets);
        Assert.Equal(2_700m, result.TotalLiabilities);
        Assert.Equal(9_600m, result.NetWorth);
    }

    [Fact]
    public void NegativeAssetReducesNetWorthWithoutReclassification()
    {
        NetWorthResult result = NetWorthCalculator.Calculate(
        [
            new(AccountClassification.Asset, -200m),
        ]);

        Assert.Equal(-200m, result.TotalAssets);
        Assert.Equal(-200m, result.NetWorth);
    }

    [Fact]
    public void NegativeLiabilityCreditIncreasesNetWorth()
    {
        NetWorthResult result = NetWorthCalculator.Calculate(
        [
            new(AccountClassification.Liability, -50m),
        ]);

        Assert.Equal(-50m, result.TotalLiabilities);
        Assert.Equal(50m, result.NetWorth);
    }

    [Fact]
    public void ProfitEstimateSubtractsRecordedExpenses()
    {
        Assert.Equal(
            1_100m,
            ProfitEstimateCalculator.Calculate(
                1_400m,
                300m));
    }

    [Fact]
    public void TaxReserveUsesTheConfiguredPercentage()
    {
        Assert.Equal(
            275m,
            TaxReserveCalculator.Calculate(
                1_100m,
                25m));
    }

    [Fact]
    public void TaxReserveDoesNotBecomeNegativeForALoss()
    {
        Assert.Equal(
            0m,
            TaxReserveCalculator.Calculate(
                -400m,
                25m));
    }
}
