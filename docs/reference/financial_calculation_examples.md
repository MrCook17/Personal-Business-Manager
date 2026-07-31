# Financial Calculation Examples

**Project:** Personal Business Manager  
**Decision:** P1-06 — Create agreed worked financial calculation examples  
**Document status:** Approved Phase 1 calculation baseline  
**Date:** 29 July 2026  
**Owner:** Charlie Cook  
**Default currency and locale:** GBP (`en-GB`)  
**Repository path:** `docs/reference/financial_calculation_examples.md`  
**Related documents:** `personal_business_management_application_final_plan.md`, `workflow_codes.md`, `schema_review.md`

---

## 1. Purpose

This document is the source of truth for implementing and testing the Personal Business Manager’s MVP financial and time calculations.

It provides exact:

- inputs;
- calculation order;
- intermediate values;
- rounding points;
- expected stored values;
- expected statuses;
- reporting results;
- edge-case behaviour.

A developer should be able to implement the calculation classes and tests without inventing a business rule.

These are management and planning calculations. They are not professional accounting, tax or regulated financial advice.

---

# 2. Global calculation rules

## 2.1 C# numeric type

All monetary, quantity, rate and percentage calculations use:

```csharp
decimal
```

Never use:

```csharp
float
double
```

for financial calculations.

Time durations use integer seconds:

```csharp
long
```

or another integer type capable of safely representing the approved duration range.

## 2.2 Stored precision

| Value | Stored precision |
|---|---:|
| Monetary totals and balances | 2 decimal places |
| Quantity and unit rate | Up to 4 decimal places |
| Percentages and rates | Up to 4 decimal places |
| Raw time duration | Whole seconds |
| Rounded time duration | Whole seconds |
| Billed time duration | Whole seconds |

Typical MariaDB types:

```text
DECIMAL(18,2)  money and balances
DECIMAL(18,4)  quantities and unit rates
DECIMAL(7,4)   percentages and rates
BIGINT         duration seconds
```

## 2.3 Monetary rounding method

Every monetary rounding operation uses:

```csharp
MidpointRounding.AwayFromZero
```

Helper:

```csharp
private static decimal RoundMoney(decimal value)
{
    return decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
```

Examples:

```text
 1.004 ->  1.00
 1.005 ->  1.01
 1.006 ->  1.01
-1.004 -> -1.00
-1.005 -> -1.01
-1.006 -> -1.01
```

Negative ordinary invoice lines are not supported in the MVP, but the shared rounding helper must still behave correctly for negative report differences and future controlled adjustments.

## 2.4 Invoice-line calculation order

### VAT-exclusive price entry

When:

```text
prices_include_vat = 0
```

the entered unit rate excludes VAT.

Calculation order:

```text
base_amount =
    RoundMoney(quantity × unit_rate)

discount_amount =
    calculate and round the approved discount against base_amount

net_amount =
    base_amount - discount_amount

vat_amount =
    RoundMoney(net_amount × vat_rate / 100)

gross_amount =
    net_amount + vat_amount
```

### VAT-inclusive price entry

When:

```text
prices_include_vat = 1
```

the entered unit rate includes VAT.

Calculation order:

```text
base_gross_amount =
    RoundMoney(quantity × unit_rate)

discount_amount =
    calculate and round the approved discount against base_gross_amount

gross_amount =
    base_gross_amount - discount_amount

net_amount =
    RoundMoney(gross_amount / (1 + vat_rate / 100))

vat_amount =
    gross_amount - net_amount
```

For a 0% VAT line:

```text
net_amount = gross_amount
vat_amount = 0.00
```

## 2.5 Discount calculation

Approved line discount types:

```text
none
percentage
fixed_amount
```

### No discount

```text
discount_amount = 0.00
```

### Percentage discount

```text
discount_amount =
    RoundMoney(base_amount × percentage / 100)
```

The percentage must be:

```text
0.0000 to 100.0000 inclusive
```

### Fixed discount

```text
discount_amount =
    RoundMoney(fixed_discount_value)
```

For an ordinary invoice line, the fixed discount cannot exceed the line’s base amount.

### Discount basis

The discount uses the same basis as the entered price:

```text
VAT-exclusive invoice -> discount is a net discount
VAT-inclusive invoice -> discount is a gross discount
```

This prevents an ambiguous VAT allocation.

## 2.6 Ordinary line sign policy

For the MVP:

- quantities are nonnegative;
- unit rates are nonnegative;
- discounts are nonnegative;
- net, VAT and gross line amounts are nonnegative;
- an `adjustment` line may add a positive charge;
- an ordinary invoice does not use a negative adjustment as a hidden credit;
- reductions before finalisation use line discounts;
- reductions after finalisation use a credit note.

Credit-note lines store positive quantities, rates and amounts. The `credit_note` document type supplies the reversing financial meaning.

## 2.7 Line and invoice totals

Each line stores its individually rounded:

```text
net_amount
vat_amount
gross_amount
discount_amount
```

Invoice totals are the sum of the stored rounded line values:

```text
invoice.net_total =
    sum(invoice_lines.net_amount)

invoice.vat_total =
    sum(invoice_lines.vat_amount)

invoice.gross_total =
    sum(invoice_lines.gross_amount)
```

Do not calculate invoice VAT by applying one rate to the invoice-wide net total.

Do not recalculate historical finalised totals from current rates or customer settings.

## 2.8 Time-based line amount

A time line uses exact billed seconds:

```text
unrounded_billed_amount =
    billed_seconds × billed_rate / 3600
```

Then:

```text
billed_amount =
    RoundMoney(unrounded_billed_amount)
```

Do not first round the derived decimal hours to two decimal places.

Example:

```text
3,900 seconds / 3,600 = 1.083333... hours
1.083333... × £45.00 = £48.75
```

Incorrect approach:

```text
1.08 hours × £45.00 = £48.60
```

Expected amount:

```text
£48.75
```

## 2.9 Finalised document immutability

Once an invoice or credit note is finalised, the following are immutable:

- document number;
- customer billing snapshot;
- line quantities;
- line unit rates;
- discounts;
- VAT rates;
- line totals;
- invoice totals;
- billed duration and rate snapshots.

Corrections use:

- payment reversal;
- credit note;
- replacement document;
- another explicitly approved audited workflow.

## 2.10 Configurable rates

The following examples use a 20% VAT rate because it is a clear worked example.

The application must not hard-code 20% permanently.

VAT registration state, VAT rates and tax-reserve percentages remain configurable.

---

# 3. Invoice calculation examples

## 3.1 Quantity multiplied by unit rate

### Inputs

```text
Quantity:              3.0000
Unit rate excluding VAT: £42.5000
Discount:              none
VAT rate:              20.0000%
Prices include VAT:    No
```

### Calculation

```text
base_amount =
    RoundMoney(3.0000 × £42.5000)
  = RoundMoney(£127.50000000)
  = £127.50

discount_amount =
    £0.00

net_amount =
    £127.50 - £0.00
  = £127.50

vat_amount =
    RoundMoney(£127.50 × 20 / 100)
  = RoundMoney(£25.500)
  = £25.50

gross_amount =
    £127.50 + £25.50
  = £153.00
```

### Expected stored line

```text
quantity:          3.0000
unit_rate:         42.5000
discount_value:    0.0000
discount_amount:   0.00
net_amount:        127.50
vat_rate:          20.0000
vat_amount:        25.50
gross_amount:      153.00
```

---

## 3.2 Four-decimal quantity and rate precision

### Inputs

```text
Quantity:              1.3333
Unit rate excluding VAT: £27.5000
VAT rate:              20.0000%
```

### Calculation

```text
unrounded base =
    1.3333 × £27.5000
  = £36.66575000

base_amount =
    RoundMoney(£36.66575000)
  = £36.67

vat_amount =
    RoundMoney(£36.67 × 20%)
  = RoundMoney(£7.334)
  = £7.33

gross_amount =
    £36.67 + £7.33
  = £44.00
```

### Expected stored values

```text
net_amount:   36.67
vat_amount:    7.33
gross_amount: 44.00
```

The application does not round the quantity or unit rate to two decimal places before multiplication.

---

## 3.3 Percentage line discount

### Inputs

```text
Quantity:              10.0000
Unit rate excluding VAT: £12.9900
Discount type:         percentage
Discount value:        15.0000%
VAT rate:              20.0000%
```

### Calculation

```text
base_amount =
    RoundMoney(10 × £12.99)
  = £129.90

unrounded discount =
    £129.90 × 15 / 100
  = £19.485

discount_amount =
    RoundMoney(£19.485)
  = £19.49

net_amount =
    £129.90 - £19.49
  = £110.41

vat_amount =
    RoundMoney(£110.41 × 20%)
  = RoundMoney(£22.082)
  = £22.08

gross_amount =
    £110.41 + £22.08
  = £132.49
```

### Expected stored line

```text
discount_type_code: percentage
discount_value:     15.0000
discount_amount:    19.49
net_amount:         110.41
vat_amount:          22.08
gross_amount:       132.49
```

---

## 3.4 Fixed-amount line discount

### Inputs

```text
Quantity:              4.0000
Unit rate excluding VAT: £25.0000
Discount type:         fixed_amount
Discount value:        £7.50
VAT rate:              20.0000%
```

### Calculation

```text
base_amount =
    RoundMoney(4 × £25.00)
  = £100.00

discount_amount =
    RoundMoney(£7.50)
  = £7.50

net_amount =
    £100.00 - £7.50
  = £92.50

vat_amount =
    RoundMoney(£92.50 × 20%)
  = £18.50

gross_amount =
    £92.50 + £18.50
  = £111.00
```

### Expected stored values

```text
discount_value:   7.5000
discount_amount:  7.50
net_amount:      92.50
vat_amount:      18.50
gross_amount:   111.00
```

---

## 3.5 VAT is calculated after discount

### Inputs

```text
Quantity:              2.0000
Unit rate excluding VAT: £75.0000
Percentage discount:   10.0000%
VAT rate:              20.0000%
```

### Calculation

```text
base_amount =
    2 × £75.00
  = £150.00

discount_amount =
    RoundMoney(£150.00 × 10%)
  = £15.00

net_amount =
    £150.00 - £15.00
  = £135.00

vat_amount =
    RoundMoney(£135.00 × 20%)
  = £27.00

gross_amount =
    £135.00 + £27.00
  = £162.00
```

Incorrect VAT calculation:

```text
£150.00 × 20% = £30.00
```

The incorrect result taxes the pre-discount value.

Expected VAT:

```text
£27.00
```

---

## 3.6 Two-decimal monetary rounding

### Inputs

```text
Quantity:  1.0000
Unit rate: £10.0050
```

### Calculation

```text
base_amount =
    RoundMoney(1 × £10.0050)
  = £10.01
```

Expected stored monetary amount:

```text
10.01
```

The unit rate may retain:

```text
10.0050
```

while the stored line money amount is:

```text
10.01
```

---

## 3.7 `MidpointRounding.AwayFromZero`

Required unit-test cases:

| Input | Expected rounded money |
|---:|---:|
| `1.004` | `1.00` |
| `1.005` | `1.01` |
| `1.006` | `1.01` |
| `-1.004` | `-1.00` |
| `-1.005` | `-1.01` |
| `-1.006` | `-1.01` |
| `2.675` | `2.68` |
| `10.995` | `11.00` |

Use `decimal` literals or parsed decimal values in tests.

Do not create these test values through binary floating-point conversion.

---

## 3.8 VAT-exclusive price example

### Inputs

```text
Quantity:              2.0000
Unit rate excluding VAT: £49.9950
VAT rate:              20.0000%
Prices include VAT:    No
```

### Calculation

```text
base/net amount =
    RoundMoney(2 × £49.9950)
  = RoundMoney(£99.9900)
  = £99.99

VAT =
    RoundMoney(£99.99 × 20%)
  = RoundMoney(£19.998)
  = £20.00

Gross =
    £99.99 + £20.00
  = £119.99
```

### Expected stored values

```text
net_amount:    99.99
vat_amount:    20.00
gross_amount: 119.99
```

---

## 3.9 VAT-inclusive price example

### Inputs

```text
Quantity:            3.0000
Unit rate including VAT: £39.9900
VAT rate:            20.0000%
Prices include VAT:  Yes
Discount:            none
```

### Calculation

```text
gross_amount =
    RoundMoney(3 × £39.99)
  = £119.97

unrounded net =
    £119.97 / 1.20
  = £99.975

net_amount =
    RoundMoney(£99.975)
  = £99.98

vat_amount =
    £119.97 - £99.98
  = £19.99
```

### Expected stored values

```text
net_amount:    99.98
vat_amount:    19.99
gross_amount: 119.97
```

VAT is derived as:

```text
gross - rounded net
```

This guarantees:

```text
net_amount + vat_amount = gross_amount
```

---

## 3.10 VAT-inclusive percentage discount

### Inputs

```text
Quantity:             2.0000
Unit rate including VAT: £60.0000
Percentage discount:  10.0000%
VAT rate:             20.0000%
```

### Calculation

```text
base gross =
    2 × £60.00
  = £120.00

gross discount =
    RoundMoney(£120.00 × 10%)
  = £12.00

gross after discount =
    £120.00 - £12.00
  = £108.00

net =
    RoundMoney(£108.00 / 1.20)
  = £90.00

VAT =
    £108.00 - £90.00
  = £18.00
```

### Expected stored values

```text
discount_amount: 12.00
net_amount:      90.00
vat_amount:      18.00
gross_amount:   108.00
```

Because prices include VAT, `discount_amount` is a gross discount.

---

## 3.11 Multiple lines with stored rounded totals

### Line 1 — standard VAT

```text
Quantity:    2
Unit rate:   £35.00 excluding VAT
Discount:    none
Net:         £70.00
VAT at 20%:  £14.00
Gross:       £84.00
```

### Line 2 — percentage discount

```text
Quantity:        3
Unit rate:       £12.50 excluding VAT
Base:            £37.50
Discount at 10%: £3.75
Net:             £33.75
VAT at 20%:      £6.75
Gross:           £40.50
```

### Line 3 — zero-rated

```text
Quantity:    1
Unit rate:   £20.00 excluding VAT
Net:         £20.00
VAT at 0%:   £0.00
Gross:       £20.00
```

### Invoice totals

```text
net_total =
    £70.00 + £33.75 + £20.00
  = £123.75

vat_total =
    £14.00 + £6.75 + £0.00
  = £20.75

gross_total =
    £84.00 + £40.50 + £20.00
  = £144.50
```

### Expected stored invoice

```text
net_total:   123.75
vat_total:    20.75
gross_total: 144.50
```

Control equation:

```text
123.75 + 20.75 = 144.50
```

---

## 3.12 Time-based invoice line

### Inputs

```text
Billed seconds:  3,900
Billed rate:     £45.0000 per hour
VAT rate:        20.0000%
Prices include VAT: No
```

### Calculation

```text
exact billed hours =
    3,900 / 3,600
  = 1.083333333...

unrounded billed amount =
    3,900 × £45.00 / 3,600
  = £48.75

net_amount =
    RoundMoney(£48.75)
  = £48.75

vat_amount =
    RoundMoney(£48.75 × 20%)
  = £9.75

gross_amount =
    £48.75 + £9.75
  = £58.50
```

### Expected stored snapshot

```text
billed_seconds: 3,900
billed_rate:    45.0000
billed_amount:  48.75
net_amount:     48.75
vat_amount:      9.75
gross_amount:   58.50
```

---

## 3.13 Invoice-line validation examples

The following are rejected:

| Input | Reason |
|---|---|
| Quantity `-1` | Negative ordinary quantity is not supported. |
| Unit rate `-£20` | Negative ordinary rate is not supported. |
| Percentage discount `100.0001%` | Exceeds 100%. |
| Fixed discount `£51` against a `£50` base | Discount exceeds the ordinary line base. |
| `discount_type = none` with `discount_value = 5` | Inconsistent discount state. |
| `credit` line without an original line reference | Credit-line reference required. |
| Non-credit line with an original credit reference | Invalid reference. |
| Finalise with no lines | At least one valid line is required. |
| Finalise with totals not equal to stored line sums | Document is inconsistent. |

A 100% percentage discount is valid if the resulting net, VAT and gross amounts are all `0.00`.

---

# 4. Payments and credit-note examples

## 4.1 Settlement formulas

For an original invoice:

```text
valid_payments =
    sum(non-reversed payment amounts)

finalised_credit_total =
    sum(finalised linked credit-note gross totals)

remaining_after_credit =
    max(invoice.gross_total - finalised_credit_total, 0)

outstanding_amount =
    max(remaining_after_credit - valid_payments, 0)

overpayment_amount =
    max(valid_payments - remaining_after_credit, 0)
```

The invoice’s stored `amount_paid` is the sum of non-reversed payments.

Credit notes are not payments and do not increase `amount_paid`.

## 4.2 Status precedence

For a finalised original invoice:

1. If the invoice is fully credited, status is `credited`.
2. Otherwise, if outstanding is zero because of payments, status is `paid`.
3. Otherwise, if valid payments are above zero, status is `part_paid`.
4. Otherwise, retain `sent` when `sent_utc` exists.
5. Otherwise, retain `finalised`.

`overdue` remains derived and is not stored.

---

## 4.3 No payment

### Inputs

```text
Invoice gross total:    £600.00
Finalised credit total: £0.00
Valid payments:         £0.00
Invoice has been sent:  Yes
```

### Calculation

```text
outstanding =
    max(£600.00 - £0.00 - £0.00, £0.00)
  = £600.00

overpayment =
    £0.00
```

### Expected result

```text
amount_paid:        0.00
outstanding_amount: 600.00
status_code:        sent
```

If it has not been sent:

```text
status_code: finalised
```

---

## 4.4 Part payment

### Inputs

```text
Invoice gross total: £600.00
Payment received:    £200.00
Payment reversed:    No
Credits:             £0.00
```

### Calculation

```text
valid_payments =
    £200.00

outstanding =
    £600.00 - £200.00
  = £400.00
```

### Expected result

```text
amount_paid:        200.00
outstanding_amount: 400.00
status_code:        part_paid
```

---

## 4.5 Fully paid

### Inputs

```text
Invoice gross total: £600.00
Valid payments:      £600.00
Credits:             £0.00
```

### Calculation

```text
outstanding =
    max(£600.00 - £600.00, £0.00)
  = £0.00
```

### Expected result

```text
amount_paid:        600.00
outstanding_amount:   0.00
status_code:        paid
paid_utc:           current transaction timestamp
```

---

## 4.6 Overpayment requiring confirmation

### Inputs

```text
Invoice gross total: £600.00
Proposed payment:    £625.00
Existing payments:   £0.00
Credits:             £0.00
```

### Pre-insert calculation

```text
proposed total payments =
    £625.00

remaining after credit =
    £600.00

overpayment =
    £625.00 - £600.00
  = £25.00
```

### Required confirmation

```text
This payment exceeds the outstanding balance by £25.00.

Invoice outstanding: £600.00
Payment entered:     £625.00
Overpayment:          £25.00

[Go back] [Record overpayment]
```

The payment is not inserted until the user explicitly confirms.

### Expected result if confirmed

```text
amount_paid:        625.00
outstanding_amount:   0.00
derived overpayment: 25.00
status_code:        paid
```

The £25.00 remains visible as an overpayment requiring later resolution.

Do not store a negative outstanding amount.

---

## 4.7 Partial credit note

### Inputs

```text
Original invoice gross: £600.00
Finalised credit note:  £150.00
Valid payments:         £0.00
Original invoice sent:  Yes
```

Credit-note values are positive:

```text
credit_note.gross_total = 150.00
```

### Calculation

```text
remaining after credit =
    £600.00 - £150.00
  = £450.00

outstanding =
    £450.00 - £0.00
  = £450.00
```

### Expected result

```text
amount_paid:        0.00
outstanding_amount: 450.00
status_code:        sent
credited amount:    150.00
```

A partially credited invoice does not use status `credited`.

---

## 4.8 Partial payment plus partial credit

### Inputs

```text
Original invoice gross: £600.00
Finalised credit total: £100.00
Valid payments:         £200.00
```

### Calculation

```text
remaining after credit =
    £600.00 - £100.00
  = £500.00

outstanding =
    £500.00 - £200.00
  = £300.00
```

### Expected result

```text
amount_paid:        200.00
credited amount:    100.00
outstanding_amount: 300.00
status_code:        part_paid
```

---

## 4.9 Full credit note

### Inputs

```text
Original invoice gross: £600.00
Finalised credit total: £600.00
Valid payments:         £0.00
```

### Calculation

```text
remaining after credit =
    max(£600.00 - £600.00, £0.00)
  = £0.00

outstanding =
    £0.00
```

### Expected result

```text
amount_paid:        0.00
outstanding_amount: 0.00
status_code:        credited
```

A credit above the available uncredited amount is rejected.

---

## 4.10 Payment reversal

### Before reversal

```text
Invoice gross:      £600.00
Payment:            £600.00
Payment reversed:   No
Sent timestamp:     Present
Status:             paid
Outstanding:        £0.00
```

### Reversal

Required:

```text
is_reversed = 1
reversed_utc = current UTC timestamp
reversal_reason = nonblank reason
```

The payment row remains.

### Recalculation

```text
valid_payments =
    £0.00

outstanding =
    £600.00
```

### Expected result

```text
amount_paid:        0.00
outstanding_amount: 600.00
status_code:        sent
```

If no sent timestamp exists, the recalculated status is:

```text
finalised
```

---

## 4.11 Credit and payment edge cases

| Scenario | Expected behaviour |
|---|---|
| Credit note references itself | Reject in invoice application service. |
| Credit exceeds available uncredited amount | Reject unless a future authorised override is designed. |
| Payment amount is zero | Reject. |
| Payment amount is negative | Reject; use reversal workflow. |
| Reversal reason is blank | Reject. |
| Payment against a draft | Reject. |
| Payment against a credit note | Reject in the MVP. |
| Full credit after an overpayment | Flag for manual resolution/refund workflow; do not hide the excess. |
| Repeated payment submission | Prevent through disabled UI and transaction/idempotency protection. |

---

# 5. Time calculation examples

## 5.1 Raw duration

### Inputs

```text
Start UTC: 29 July 2026 09:12:14
End UTC:   29 July 2026 10:14:44
```

### Calculation

```text
10:14:44 - 09:12:14
= 1 hour, 2 minutes, 30 seconds
= 3,750 seconds
```

### Expected stored value

```text
raw_duration_seconds: 3,750
```

Validation:

```text
end_utc > start_utc
raw_duration_seconds > 0
```

---

## 5.2 Rounding algorithms

For an interval:

```text
interval_seconds =
    interval_minutes × 60
```

### Nearest interval

```text
quotient  = raw_seconds / interval_seconds
remainder = raw_seconds % interval_seconds
```

If:

```text
remainder × 2 < interval_seconds
```

round down.

Otherwise:

```text
round up
```

This makes exact half-interval ties round upward.

### Up interval

```text
rounded_seconds =
    ceiling(raw_seconds / interval_seconds)
    × interval_seconds
```

If the duration is already an exact interval, it remains unchanged.

---

## 5.3 Every approved rounding rule

Use the same raw duration:

```text
raw_duration_seconds = 3,750
raw display = 1h 2m 30s
```

| Rule | Interval | Calculation | Expected rounded seconds | Display |
|---|---:|---|---:|---|
| `none` | None | Preserve raw | `3,750` | `1h 2m 30s` |
| `nearest_5` | 300 sec | Halfway between 60 and 65 minutes; tie upward | `3,900` | `1h 5m` |
| `nearest_6` | 360 sec | 62.5 minutes is nearer 60 than 66 | `3,600` | `1h` |
| `nearest_10` | 600 sec | 62.5 minutes is nearer 60 than 70 | `3,600` | `1h` |
| `nearest_15` | 900 sec | 62.5 minutes is nearer 60 than 75 | `3,600` | `1h` |
| `up_5` | 300 sec | Next 5-minute boundary | `3,900` | `1h 5m` |
| `up_6` | 360 sec | Next 6-minute boundary | `3,960` | `1h 6m` |
| `up_10` | 600 sec | Next 10-minute boundary | `4,200` | `1h 10m` |
| `up_15` | 900 sec | Next 15-minute boundary | `4,500` | `1h 15m` |

Expected stored record for `nearest_5`:

```text
raw_duration_seconds:     3,750
rounded_duration_seconds: 3,900
rounding_rule_code:       nearest_5
```

## 5.4 Exact-boundary upward rounding

### Inputs

```text
Raw duration: 3,600 seconds
Rule: up_15
```

### Calculation

```text
3,600 seconds = exactly 60 minutes
60 minutes is already a 15-minute boundary
```

### Expected result

```text
rounded_duration_seconds: 3,600
```

Do not round an exact interval to the next interval.

---

## 5.5 Manual time entry

### Inputs

```text
Entry date:       29 July 2026
Entered duration: 1 hour 20 minutes
Method:           manual
Rounding rule:    none
Billable:         Yes
```

### Calculation

```text
1 hour =
    3,600 seconds

20 minutes =
    1,200 seconds

raw duration =
    3,600 + 1,200
  = 4,800 seconds

rounded duration =
    4,800 seconds
```

### Expected stored values

```text
entry_method_code:       manual
raw_duration_seconds:    4,800
rounded_duration_seconds:4,800
rounding_rule_code:      none
is_billable:             1
```

The service must create valid start/end timestamps or an equivalent approved representation that agrees with the stored duration.

---

## 5.6 Billable versus non-billable

### Entry A

```text
Rounded duration: 3,600 seconds
Billable:         Yes
```

Expected:

- included in tracked-time reports;
- included in billable-time reports;
- eligible for invoice selection when otherwise valid;
- may receive a billed duration/rate snapshot.

### Entry B

```text
Rounded duration: 3,600 seconds
Billable:         No
```

Expected:

- included in tracked-time reports;
- included in non-billable/utilisation reports;
- excluded from normal invoice selection;
- cannot be invoiced unless changed through an audited correction before invoicing.

Both entries contribute:

```text
1 hour
```

to total tracked time.

Only Entry A contributes:

```text
1 hour
```

to billable time.

---

## 5.7 Billed duration and rate snapshot

### Time entry

```text
Raw duration:       3,750 seconds
Rounding rule:      nearest_5
Rounded duration:   3,900 seconds
Effective rate:     £45.0000 per hour
```

### At invoice selection/finalisation

```text
billed_seconds =
    3,900

billed_rate =
    £45.0000

unrounded billed amount =
    3,900 × £45.00 / 3,600
  = £48.75

billed_amount =
    £48.75
```

### Expected immutable snapshot

```text
invoice_time_entries.billed_seconds: 3,900
invoice_time_entries.billed_rate:    45.0000
invoice_time_entries.billed_amount:  48.75
```

Changing the customer or job default rate later does not change this snapshot.

---

## 5.8 Time validation and edge cases

| Scenario | Expected behaviour |
|---|---|
| End equals start | Reject: duration must be positive. |
| End precedes start | Reject. |
| Raw duration zero | Reject. |
| Rounding produces zero | Reject for a persisted completed entry. |
| Two active timers for one user | Database unique constraint rejects the second. |
| Stop operation partly fails | Roll back both time-entry insertion and timer deletion. |
| Correct an invoiced entry | Reject direct edit; use financial correction workflow. |
| Change billable state | Require audited correction. |
| Unknown rounding code | Reject in C# and MariaDB. |
| `none` with seconds | Preserve exact seconds; do not coerce to minutes. |

---

# 6. Reporting calculation examples

## 6.1 Example reporting dataset

Assume the business is VAT registered for this worked example.

### Finalised sales documents

| Document | Type | Document date | Net | VAT | Gross |
|---|---|---|---:|---:|---:|
| `INV-101` | Invoice | 5 July 2026 | £1,000.00 | £200.00 | £1,200.00 |
| `INV-102` | Invoice | 20 July 2026 | £500.00 | £100.00 | £600.00 |
| `CN-001` | Credit note | 25 July 2026 | £100.00 | £20.00 | £120.00 |

### Payments

| Payment | Payment date | Invoice | Amount | Reversed |
|---|---|---|---:|---|
| `PAY-1` | 10 July 2026 | `INV-101` | £600.00 | No |
| `PAY-2` | 28 July 2026 | `INV-102` | £300.00 | No |
| `PAY-3` | 2 August 2026 | `INV-101` | £600.00 | No |

### July expenses

| Expense | Expense date | Paid date/basis | Net | VAT | Gross |
|---|---|---|---:|---:|---:|
| Hosting | 12 July 2026 | July | £200.00 | £40.00 | £240.00 |
| Insurance | 18 July 2026 | July | £100.00 | £0.00 | £100.00 |

July expense totals:

```text
Net:   £300.00
VAT:    £40.00
Gross: £340.00
```

---

## 6.2 Invoiced revenue

Invoiced revenue uses finalised invoices minus finalised credit notes, grouped by document date.

Draft and cancelled documents are excluded.

### July net revenue

```text
invoice net totals =
    £1,000.00 + £500.00
  = £1,500.00

credit-note net totals =
    £100.00

net invoiced revenue =
    £1,500.00 - £100.00
  = £1,400.00
```

### July gross revenue

```text
invoice gross totals =
    £1,200.00 + £600.00
  = £1,800.00

credit-note gross totals =
    £120.00

gross invoiced revenue =
    £1,800.00 - £120.00
  = £1,680.00
```

### Expected report values

```text
Net invoiced revenue:   £1,400.00
VAT component:            £280.00
Gross invoiced revenue: £1,680.00
```

Payment dates do not affect invoiced revenue.

---

## 6.3 Received income

Received income is grouped by payment date and includes only non-reversed payments.

Credit notes are not payments.

### July

```text
10 July payment =
    £600.00

28 July payment =
    £300.00

July received income =
    £600.00 + £300.00
  = £900.00
```

### August

```text
2 August payment =
    £600.00

August received income =
    £600.00
```

### Expected report

```text
July 2026 received income:   £900.00
August 2026 received income: £600.00
```

The fact that `INV-101` was dated in July does not move its August payment into July received income.

A reversed payment is excluded from received income.

An accepted overpayment is included in cash received and separately identified as an overpayment.

---

## 6.4 Invoiced profit estimate

For a VAT-registered net-basis management estimate:

```text
invoiced_profit_estimate =
    net invoiced revenue
    - net recorded business expenses
```

Using the dataset:

```text
£1,400.00 - £300.00
= £1,100.00
```

### Expected result

```text
July net-basis invoiced profit estimate: £1,100.00
```

For a gross-basis view:

```text
£1,680.00 - £340.00
= £1,340.00
```

The report must label its selected basis.

Do not present either figure as statutory accounting profit.

---

## 6.5 Cash profit estimate

The MVP cash-profit estimate follows the approved gross cash formula:

```text
cash_profit_estimate =
    received income
    - recorded paid expenses
```

Using July cash:

```text
received income =
    £900.00

paid expenses gross =
    £340.00

cash profit estimate =
    £900.00 - £340.00
  = £560.00
```

### Expected result

```text
July gross cash profit estimate: £560.00
```

This is a management cash estimate, not accounting profit.

The report must state:

- payment-date basis;
- gross cash basis;
- selected date range;
- that unpaid invoices are excluded;
- that unpaid expenses are excluded.

---

## 6.6 Tax-reserve estimate

Configured tax-reserve percentage:

```text
25.0000%
```

Selected estimated profit:

```text
£1,100.00
```

Formula:

```text
tax_reserve =
    max(estimated_profit, £0.00)
    × configured_percentage / 100
```

Calculation:

```text
max(£1,100.00, £0.00)
× 25%
= £275.00
```

### Expected result

```text
Estimated tax reserve: £275.00
```

### Loss example

```text
Estimated profit: -£400.00

max(-£400.00, £0.00)
= £0.00

Tax reserve:
£0.00 × 25%
= £0.00
```

The percentage is configurable and must not be hard-coded.

Permanent notice:

> Planning estimate only. This is not a tax calculation or a replacement for professional accounting advice.

---

## 6.7 VAT estimate

Output VAT uses finalised invoices minus finalised credit notes.

Input VAT uses recorded expense VAT within the selected reporting basis.

### Output VAT

```text
Invoice VAT =
    £200.00 + £100.00
  = £300.00

Credit-note VAT =
    £20.00

Output VAT after credits =
    £300.00 - £20.00
  = £280.00
```

### Input VAT

```text
Hosting input VAT =
    £40.00

Insurance input VAT =
    £0.00

Total input VAT =
    £40.00
```

### Estimated VAT position

```text
£280.00 - £40.00
= £240.00
```

### Expected result

```text
Output VAT:            £280.00
Input VAT recorded:     £40.00
Estimated VAT payable: £240.00
```

If input VAT exceeds output VAT, show a negative position clearly as an estimated reclaim/credit position rather than forcing it to zero.

Do not present this result as a submitted VAT return.

---

## 6.8 Reporting exclusions and date rules

| Measure | Date basis | Includes |
|---|---|---|
| Invoiced revenue | Invoice/credit-note document date | Finalised invoices minus finalised credit notes |
| Received income | Payment date | Non-reversed payments |
| Invoiced profit estimate | Document/expense date | Selected revenue basis minus selected expense basis |
| Cash profit estimate | Payment/paid-expense date | Cash received minus paid expense cash |
| VAT estimate | Configured reporting basis | Output VAT minus recorded input VAT |
| Tax reserve | Selected estimated profit | Configured percentage of positive estimate |

Draft, cancelled and reversed items are excluded according to their domain rules.

Business reports must not include personal account balances or personal contributions.

---

# 7. Personal-finance calculation examples

## 7.1 General net-worth rule

Each account has a classification:

```text
asset
liability
```

Formula:

```text
net_worth =
    sum(signed asset balances)
    - sum(signed liability balances)
```

For liabilities:

- positive balance means money owed;
- negative balance means a credit in the user’s favour.

`available_balance` is not used for net worth.

The latest recorded current balance is used.

Hidden accounts remain included because hiding is a display preference.

An account should not be archived with an unresolved nonzero current balance.

---

## 7.2 Positive asset

### Inputs

```text
Account type:     savings_account
Classification:   asset
Current balance:  £10,000.00
```

### Contribution to net worth

```text
+£10,000.00
```

Expected asset total effect:

```text
£10,000.00
```

---

## 7.3 Negative current-account asset balance

### Inputs

```text
Account type:     current_account
Classification:   asset
Current balance:  -£200.00
```

### Contribution to asset total

```text
-£200.00
```

### Contribution to net worth

```text
-£200.00
```

The application does not automatically reclassify the account as a liability.

The negative signed balance correctly reduces net worth.

---

## 7.4 Positive liability

### Inputs

```text
Account type:     credit_card
Classification:   liability
Current balance:  £750.00
```

### Net-worth calculation

```text
net-worth contribution =
    -£750.00
```

The liability total increases by:

```text
£750.00
```

---

## 7.5 Negative liability or credit balance

### Inputs

```text
Account type:     credit_card
Classification:   liability
Current balance:  -£50.00
```

This represents a £50 credit.

### Net-worth calculation

```text
subtract liability balance =
    -(-£50.00)
  = +£50.00
```

Expected effect on net worth:

```text
+£50.00
```

---

## 7.6 Complete net-worth total

### Asset accounts

| Account | Classification | Balance |
|---|---|---:|
| Current account | Asset | £2,500.00 |
| Savings account | Asset | £10,000.00 |
| Overdrawn current account | Asset | -£200.00 |

Asset sum:

```text
£2,500.00 + £10,000.00 - £200.00
= £12,300.00
```

### Liability accounts

| Account | Classification | Balance |
|---|---|---:|
| Credit card | Liability | £750.00 |
| Personal loan | Liability | £2,000.00 |
| Credit-balance card | Liability | -£50.00 |

Signed liability sum:

```text
£750.00 + £2,000.00 - £50.00
= £2,700.00
```

### Net worth

```text
£12,300.00 - £2,700.00
= £9,600.00
```

### Expected report

```text
Total assets:      £12,300.00
Total liabilities: £2,700.00
Estimated net worth: £9,600.00
```

The displayed liability total is the signed sum used in the calculation.

A separate breakdown makes the negative liability credit visible so the total is not misleading.

---

## 7.7 Contributions do not automatically change balance

### Starting account

```text
Account:          Lifetime ISA
Current balance:  £10,000.00
```

### New contribution record

```text
Contribution date: 29 July 2026
Contribution type: personal_contribution
Amount:            £500.00
Tax year:          2026/27
```

### Expected effect immediately after saving the contribution

```text
Contribution total for 2026/27 increases by: £500.00
Current account balance remains:             £10,000.00
Net-worth contribution remains:              £10,000.00
```

The contribution does not become:

```text
£10,500.00
```

until a separate balance update creates a new balance snapshot.

This avoids double counting where the provider’s reported balance already includes the contribution.

---

## 7.8 Balance snapshot update

### Before

```text
financial_accounts.current_balance: £10,000.00
```

### New snapshot

```text
balance_at_utc:      29 July 2026 18:45 UTC
balance_amount:      £10,525.00
snapshot_source:     statement
```

### One transaction

1. Insert snapshot for £10,525.00.
2. Update current balance to £10,525.00.
3. Update last-balance timestamp.
4. Create audit record.
5. Commit.

### Expected result

```text
Current balance:       £10,525.00
Latest snapshot:       £10,525.00
Net-worth contribution:£10,525.00
```

The previously recorded £500 contribution remains informational and is not added again.

---

## 7.9 Personal-finance validation examples

| Scenario | Expected behaviour |
|---|---|
| Asset balance is negative | Allow; signed value reduces net worth. |
| Liability balance is negative | Allow; signed credit increases net worth. |
| Available balance differs from current balance | Allow; net worth uses current balance. |
| Contribution is recorded | Do not alter current balance. |
| Balance snapshot saved but account update fails | Roll back both operations. |
| Account is hidden | Keep it in net worth. |
| Personal report receives a business account | Reject scope mismatch. |
| Liability uses asset sign logic | Unit test must fail. |
| Closed account has nonzero balance | Display clearly; require correction before archive. |

---

# 8. Consolidated expected unit-test cases

## 8.1 Invoice tests

| Test | Expected result |
|---|---|
| `ThreeUnitsAt42_50_ExclusiveVat` | Net `127.50`, VAT `25.50`, gross `153.00` |
| `FourDecimalQuantityPreservedUntilMoneyRounding` | Net `36.67`, VAT `7.33`, gross `44.00` |
| `PercentageDiscountRoundsBeforeNet` | Discount `19.49`, net `110.41`, VAT `22.08`, gross `132.49` |
| `FixedDiscountAppliedBeforeVat` | Net `92.50`, VAT `18.50`, gross `111.00` |
| `VatCalculatedAfterDiscount` | VAT `27.00`, gross `162.00` |
| `MidpointRoundsAwayFromZero` | `1.005 -> 1.01`, `-1.005 -> -1.01` |
| `VatExclusiveLineRoundsVatSeparately` | Net `99.99`, VAT `20.00`, gross `119.99` |
| `VatInclusiveLineDerivesNetAndVat` | Net `99.98`, VAT `19.99`, gross `119.97` |
| `MultipleLineTotalsUseStoredRoundedLines` | Net `123.75`, VAT `20.75`, gross `144.50` |
| `TimeLineUsesExactBilledSeconds` | Net `48.75`, VAT `9.75`, gross `58.50` |

## 8.2 Settlement tests

| Test | Expected result |
|---|---|
| `NoPaymentKeepsSentStatus` | Outstanding `600.00`, status `sent` |
| `PartPaymentSetsPartPaid` | Paid `200.00`, outstanding `400.00` |
| `FullPaymentSetsPaid` | Paid `600.00`, outstanding `0.00` |
| `OverpaymentRequiresConfirmation` | Excess `25.00` before insert |
| `ConfirmedOverpaymentNeverCreatesNegativeOutstanding` | Outstanding `0.00`, overpayment `25.00` |
| `PartialCreditReducesOutstanding` | Outstanding `450.00`, not `credited` |
| `FullCreditSetsCredited` | Outstanding `0.00`, status `credited` |
| `ReversedPaymentExcludedFromPaidTotal` | Paid `0.00`, outstanding `600.00` |

## 8.3 Time tests

| Test | Expected rounded seconds |
|---|---:|
| `NonePreserves3750Seconds` | `3,750` |
| `Nearest5TieRoundsUp` | `3,900` |
| `Nearest6RoundsDown` | `3,600` |
| `Nearest10RoundsDown` | `3,600` |
| `Nearest15RoundsDown` | `3,600` |
| `Up5RoundsTo3900` | `3,900` |
| `Up6RoundsTo3960` | `3,960` |
| `Up10RoundsTo4200` | `4,200` |
| `Up15RoundsTo4500` | `4,500` |
| `UpRuleLeavesExactBoundaryUnchanged` | `3,600` |

## 8.4 Reporting tests

| Test | Expected result |
|---|---:|
| `JulyNetInvoicedRevenueAfterCredit` | `£1,400.00` |
| `JulyGrossInvoicedRevenueAfterCredit` | `£1,680.00` |
| `JulyReceivedIncomeUsesPaymentDate` | `£900.00` |
| `AugustReceivedIncomeUsesPaymentDate` | `£600.00` |
| `NetInvoicedProfitEstimate` | `£1,100.00` |
| `GrossCashProfitEstimate` | `£560.00` |
| `TaxReserveAt25Percent` | `£275.00` |
| `TaxReserveDoesNotGoNegative` | `£0.00` |
| `VatPositionSubtractsCreditAndInputVat` | `£240.00` |

## 8.5 Personal-finance tests

| Test | Expected effect/result |
|---|---:|
| `PositiveAssetIncreasesNetWorth` | `+£10,000.00` |
| `NegativeAssetReducesNetWorth` | `-£200.00` |
| `PositiveLiabilityReducesNetWorth` | `-£750.00` |
| `NegativeLiabilityIncreasesNetWorth` | `+£50.00` |
| `MixedAccountsCalculateNetWorth` | `£9,600.00` |
| `ContributionDoesNotChangeBalance` | Balance remains `£10,000.00` |
| `BalanceSnapshotUpdatesCurrentBalanceAtomically` | Balance becomes `£10,525.00` |
| `HiddenAccountRemainsInNetWorth` | Included |

---

# 9. Integration-test requirements

MariaDB integration tests should prove:

- invoice line and total constraints accept the approved examples;
- inconsistent totals are rejected;
- invalid discount combinations are rejected;
- unknown code values are rejected;
- duplicate invoice numbers are rejected;
- duplicate time-entry invoice links are rejected;
- only one active timer exists per user;
- time duration seconds are stored exactly;
- payment reversal requires a nonblank reason;
- a credit note cannot reference itself through application-service validation;
- expense and invoice financial equations remain enforced;
- balance snapshot and account balance update commit or roll back together;
- test execution cannot target the normal development database.

Use a dedicated test database protected by the P2-07 guard.

---

# 10. Implementation boundaries

## 10.1 Core project

Core contains pure calculation and workflow classes such as:

```text
MoneyRounding
InvoiceLineCalculator
InvoiceTotalCalculator
InvoiceSettlementCalculator
TimeRoundingCalculator
TimeBillingCalculator
BusinessReportCalculator
TaxReserveCalculator
VatEstimateCalculator
NetWorthCalculator
```

Names may vary, but responsibilities remain separated and testable.

Core must not reference:

- WinForms;
- Dapper;
- MySqlConnector;
- MariaDB-specific classes;
- QuestPDF.

## 10.2 Application services

Services:

- load and validate current records;
- invoke Core calculations;
- control transactions;
- persist stored rounded values;
- enforce workflow transitions;
- create audit records;
- protect against duplicate submission.

## 10.3 WinForms

Forms and pages:

- gather input;
- show calculation previews from Core/service results;
- display validation;
- disable duplicate actions;
- never contain duplicate financial formulas;
- never calculate final invoice totals independently.

## 10.4 Reporting

Reports use persisted finalised values.

They do not rebuild historic invoices from current:

- VAT rates;
- customer defaults;
- job rates;
- account settings.

---

# 11. P1-06 verification checklist

## Invoice calculations

- [x] Quantity × unit rate example exists.
- [x] Four-decimal quantity/rate example exists.
- [x] Percentage line discount example exists.
- [x] Fixed-amount line discount example exists.
- [x] VAT-after-discount example exists.
- [x] Two-decimal monetary rounding is defined.
- [x] `MidpointRounding.AwayFromZero` is defined.
- [x] Multiple-line stored-total example exists.
- [x] VAT-inclusive pricing example exists.
- [x] VAT-exclusive pricing example exists.
- [x] Time-based invoice-line example exists.
- [x] Negative ordinary adjustment policy is decided.

## Payments and credit notes

- [x] No-payment example exists.
- [x] Part-payment example exists.
- [x] Fully-paid example exists.
- [x] Overpayment confirmation example exists.
- [x] Partial credit-note example exists.
- [x] Full credit-note example exists.
- [x] Payment-reversal example exists.
- [x] Combined payment-and-credit example exists.
- [x] Settlement status precedence is defined.

## Time calculations

- [x] Raw duration example exists.
- [x] `none` rounding example exists.
- [x] `nearest_5` example exists.
- [x] `nearest_6` example exists.
- [x] `nearest_10` example exists.
- [x] `nearest_15` example exists.
- [x] `up_5` example exists.
- [x] `up_6` example exists.
- [x] `up_10` example exists.
- [x] `up_15` example exists.
- [x] Exact-boundary behaviour is defined.
- [x] Manual-entry example exists.
- [x] Billable/non-billable example exists.
- [x] Billed-duration/rate snapshot exists.

## Reporting

- [x] Invoiced-revenue example exists.
- [x] Received-income example exists.
- [x] Invoiced-profit estimate exists.
- [x] Cash-profit estimate exists.
- [x] Tax-reserve estimate exists.
- [x] VAT estimate exists.
- [x] Report date bases are defined.
- [x] Net/gross basis is labelled.

## Personal finance

- [x] Positive-asset example exists.
- [x] Negative-current-account asset example exists.
- [x] Positive-liability example exists.
- [x] Negative-liability credit example exists.
- [x] Complete net-worth example exists.
- [x] Contribution non-balance behaviour is defined.
- [x] Balance-snapshot update example exists.
- [x] Hidden-account behaviour is defined.

## Evidence still required during Phase 2

- [x] Add matching Core calculation classes.
- [x] Add matching unit tests.
- [x] Add MariaDB constraint/integration tests.
- [x] Verify the examples against implemented code.
- [x] Commit this document to Git.

---

# 12. Final decision

```text
Monetary rounding rules:             APPROVED
Invoice-line calculation order:      APPROVED
VAT-inclusive calculation:           APPROVED
VAT-exclusive calculation:           APPROVED
Discount calculation:                APPROVED
Ordinary line sign policy:           APPROVED
Invoice-total calculation:           APPROVED
Payment and credit settlement:       APPROVED
Time-rounding rules:                  APPROVED
Time billing snapshot:               APPROVED
Reporting formulas and date bases:   APPROVED
Net-worth sign handling:             APPROVED
Contribution behaviour:              APPROVED
P1-06 documentation gate:            PASS
Matching Phase 2 implementation:     PENDING
```

The examples contain exact inputs, calculation steps and expected results.

A developer should be able to implement the calculations without guessing.

---

## 13. Approval record

**Owner:** Charlie Cook  
**Approval date:** 29 July 2026  
**Status:** Approved Phase 1 calculation baseline

Any future change to calculation order, rounding, settlement status precedence, time rounding, reporting basis or net-worth sign handling must update:

1. this document;
2. Core unit tests;
3. relevant integration tests;
4. affected migrations or constraints;
5. the final development plan change log where the product rule changes.
