# Workflow and Code Values

**Project:** Personal Business Manager  
**Decision:** P1-02 — Finalise all workflow and code values  
**Date:** 29 July 2026  
**Owner:** Charlie Cook  
**Status:** Approved  
**Repository path:** `docs/reference/workflow_codes.md`

---

## 1. Purpose

This document is the single source of truth for persisted workflow codes, status codes and other controlled code values used by the Personal Business Manager.

All persisted codes:

- use lowercase `snake_case`;
- are stored in MariaDB `VARCHAR` columns unless a foreign-key lookup table is explicitly used;
- are represented by shared C# constants;
- are validated by the Core/application layer;
- use MariaDB constraints where the vocabulary is small, stable and closed;
- must not be entered as unrestricted free text in the UI.

A code must not be added, renamed or removed without updating:

1. this document;
2. the corresponding C# constants and validation;
3. workflow-transition tests;
4. MariaDB constraints or lookup seed data where applicable;
5. migrations and relevant documentation.

Released migrations must never be edited. Changes after the baseline must use a new migration.

---

## 2. Decisions

### 2.1 C# representation

**Decision:** Use static C# classes containing string constants and an `All` set for validation.

Example:

```csharp
public static class JobStatusCodes
{
    public const string Planned = "planned";
    public const string Active = "active";
    public const string OnHold = "on_hold";
    public const string Completed = "completed";
    public const string Cancelled = "cancelled";

    public static IReadOnlySet<string> All { get; } =
        new HashSet<string>(StringComparer.Ordinal)
        {
            Planned,
            Active,
            OnHold,
            Completed,
            Cancelled
        };
}
```

Reasons:

- persisted MariaDB values remain explicit and readable;
- Dapper can map the values without custom enum converters;
- no numeric enum value can accidentally be persisted;
- adding a new code does not reorder or renumber existing values;
- the database and C# values can match exactly;
- it is simpler than creating strongly typed value objects for every small code set.

Do not scatter string literals throughout forms, services, repositories or tests. Persisted code values must come from the shared Core constants.

### 2.2 Enums

Do not use ordinary C# enums as the persistence model for these codes.

Enums may be used for UI-only concepts that are never persisted, but a persisted code must remain an explicit string constant.

### 2.3 Strongly typed value objects

Do not introduce strongly typed value objects for every code in the MVP. This would add disproportionate mapping and boilerplate for a single-user application.

A value object may be introduced later for a high-risk domain concept if it provides clear validation or behaviour beyond code membership.

### 2.4 Validation layers

**Decision:** Validate closed workflow codes in both C# and MariaDB.

Use:

- UI controls to present valid choices;
- Core/application services to validate membership and legal transitions;
- MariaDB `CHECK` constraints to reject invalid closed-set values;
- foreign-key lookup tables for data-driven sets;
- integration tests to prove database enforcement.

MariaDB constraints protect against:

- application defects;
- manual SQL errors;
- import errors;
- future code paths that bypass expected UI validation.

C# remains responsible for contextual rules and legal transitions. A database check can prove that `completed` is a known code, but the application service must decide whether a particular job may currently become completed.

### 2.5 Closed versus extensible code sets

Use MariaDB `CHECK` constraints for small, closed sets such as:

- job statuses;
- priorities;
- charging types;
- task statuses;
- invoice types and statuses;
- invoice line and discount types;
- time-entry methods and rounding rules;
- financial-account classifications, scopes and statuses;
- account-application statuses;
- backup types and statuses;
- application-setting value types.

Do not use rigid closed checks for values designed to be data-driven or standards-based, including:

- `financial_account_types.account_type_code`, which is controlled by lookup rows;
- ISO country codes;
- ISO currency codes;
- audit entity and action codes, which must be centrally controlled in C# but extensible as modules are added.

---

# 3. Customer lifecycle and codes

## 3.1 Customer active and archive behaviour

Customers do not use a workflow `status_code`.

They use:

```text
is_active
date_archived_utc
```

### New customer

```text
is_active = 1
date_archived_utc = NULL
```

### Archive customer

```text
is_active = 0
date_archived_utc = current UTC timestamp
```

Archive rules:

- archived customers are excluded from normal active lists;
- historical jobs, invoices, payments, time entries, attachments and audit records remain available;
- new jobs cannot normally be created for an archived customer;
- archiving never physically deletes the customer.

### Restore customer

```text
is_active = 1
date_archived_utc = NULL
```

Restoration must create an audit record.

### Required consistency rule

The application service and database should enforce:

```text
active customer   = is_active = 1 and date_archived_utc is null
archived customer = is_active = 0 and date_archived_utc is not null
```

The same archive convention should be used by other archive-enabled records unless their module explicitly documents a different rule.

## 3.2 Customer VAT treatment codes

Column:

```text
customers.default_vat_treatment_code
```

Allowed values:

```text
standard
zero_rated
exempt
outside_scope
```

Meaning:

| Code | Meaning |
|---|---|
| `standard` | Normal configured VAT treatment applies. |
| `zero_rated` | Taxable supply with a zero VAT rate. |
| `exempt` | VAT-exempt treatment applies. |
| `outside_scope` | The transaction is outside the scope of VAT. |

`NULL` means use the application default rather than a customer-specific override.

Do not add `reverse_charge` until a tested reverse-charge workflow and invoice wording are designed.

## 3.3 Invoice delivery codes

Column:

```text
customers.invoice_delivery_code
```

Allowed values:

```text
email
post
both
manual
```

`manual` means the application records no automatic delivery instruction and the user handles delivery outside the application.

`NULL` means use the application default.

Automatic email sending remains later scope; recording a delivery preference does not implement email delivery.

## 3.4 Customer address types

Column:

```text
customer_addresses.address_type_code
```

Allowed values:

```text
billing
service
registered
other
```

`service` covers job or site addresses.

Only one active default address per customer and address type should normally exist. This is enforced transactionally in the customer service.

---

# 4. Job codes and workflow

## 4.1 Job status codes

Column:

```text
jobs.status_code
```

Allowed values:

```text
planned
active
on_hold
completed
cancelled
```

Default:

```text
planned
```

Definitions:

| Code | Meaning |
|---|---|
| `planned` | Approved or recorded work that has not started. |
| `active` | Work is currently available and in progress. |
| `on_hold` | Work is temporarily paused but not finished or cancelled. |
| `completed` | Work has finished. |
| `cancelled` | Work will not proceed or has been abandoned. |

### Normal transitions

```text
planned   -> active, on_hold, cancelled
active    -> on_hold, completed, cancelled
on_hold   -> active, completed, cancelled
completed -> active only through an explicit audited reopen
cancelled -> planned only through an explicit audited reopen
```

Rules:

- completing a job sets `completed_utc`;
- reopening a completed job clears `completed_utc` and requires an audit reason;
- cancelling does not archive or delete the job;
- a job cannot be archived while it has an active timer;
- completed and cancelled jobs reject new timers unless deliberately reopened.

## 4.2 Priority codes

Columns:

```text
jobs.priority_code
tasks.priority_code
```

Allowed values:

```text
low
normal
high
urgent
```

Default:

```text
normal
```

Priority affects ordering, filtering and presentation. It does not bypass validation or automatically change due dates.

## 4.3 Charging type codes

Column:

```text
jobs.charging_type_code
```

Allowed values:

```text
hourly
fixed_price
mixed
non_billable
```

Definitions:

| Code | Meaning |
|---|---|
| `hourly` | Invoice value is primarily calculated from billable time. |
| `fixed_price` | Work is billed using an agreed fixed amount. |
| `mixed` | Time-based and fixed/manual invoice lines may both be used. |
| `non_billable` | The job is tracked but is not normally invoiceable. |

No database default is approved. The user must choose a charging type when creating a job.

Rules:

- `fixed_price` requires a valid fixed price before invoicing;
- `hourly` requires a resolvable hourly rate before finalisation;
- `non_billable` prevents normal billable time and invoice selection;
- changing charging type after billing has started requires validation and audit where it affects financial history.

---

# 5. Time-entry and task codes

## 5.1 Time-entry method codes

Column:

```text
time_entries.entry_method_code
```

Allowed MVP values:

```text
timer
manual
```

Definitions:

| Code | Meaning |
|---|---|
| `timer` | Created by stopping a persistent active timer. |
| `manual` | Entered manually using timestamps or a date and duration. |

Corrections do not change the original entry method. A corrected timer entry remains `timer`; the audit record records the correction.

Do not add `import` until a real time-import feature exists.

No database default is approved because the creation path must explicitly supply the method.

## 5.2 Time-rounding rule codes

Column:

```text
time_entries.rounding_rule_code
```

Allowed values:

```text
none
nearest_5
nearest_6
nearest_10
nearest_15
up_5
up_6
up_10
up_15
```

Default:

```text
none
```

Definitions:

| Code | Rule |
|---|---|
| `none` | Preserve the exact raw duration; do not apply an interval. |
| `nearest_5` | Round to the nearest 5 minutes. |
| `nearest_6` | Round to the nearest 6 minutes. |
| `nearest_10` | Round to the nearest 10 minutes. |
| `nearest_15` | Round to the nearest 15 minutes. |
| `up_5` | Always round upward to the next 5-minute interval. |
| `up_6` | Always round upward to the next 6-minute interval. |
| `up_10` | Always round upward to the next 10-minute interval. |
| `up_15` | Always round upward to the next 15-minute interval. |

For nearest-interval rules, exact half-interval ties round upward.

Rounding is applied to the individual time entry. The raw duration must always remain stored.

### Required schema clarification

The current schema stores:

```text
raw_duration_seconds
rounded_duration_minutes
```

An integer `rounded_duration_minutes` cannot represent exact `none` rounding for durations containing seconds.

Before the Phase 1 schema baseline is formally approved, choose one of these implementations:

**Recommended:**

```text
replace rounded_duration_minutes with rounded_duration_seconds
```

or add:

```text
rounded_duration_seconds BIGINT UNSIGNED NOT NULL
```

Then derive display minutes/hours in C#.

This preserves exact unrounded time and gives every interval rule an unambiguous stored result.

Until this is resolved, `raw_duration_seconds` remains the authoritative value for `none`.

## 5.3 Task status codes

Column:

```text
tasks.status_code
```

Allowed values:

```text
not_started
in_progress
blocked
completed
cancelled
```

Default:

```text
not_started
```

Definitions:

| Code | Meaning |
|---|---|
| `not_started` | Task exists but work has not begun. |
| `in_progress` | Task is actively being worked on. |
| `blocked` | Task cannot currently proceed. |
| `completed` | Required work has finished. |
| `cancelled` | Task is no longer required. |

### Normal transitions

```text
not_started -> in_progress, blocked, completed, cancelled
in_progress -> blocked, completed, cancelled
blocked     -> not_started, in_progress, completed, cancelled
completed   -> not_started only through an explicit reopen
cancelled   -> not_started only through an explicit reopen
```

Completing sets `completed_utc`. Reopening clears it.

Archiving remains separate from task status.

---

# 6. Invoice, credit-note and payment codes

## 6.1 Invoice type codes

Column:

```text
invoices.invoice_type_code
```

Allowed values:

```text
invoice
credit_note
```

Default:

```text
invoice
```

A `credit_note` must reference the original invoice.

## 6.2 Invoice status codes

Column:

```text
invoices.status_code
```

Allowed values:

```text
draft
finalised
sent
part_paid
paid
cancelled
credited
```

Default:

```text
draft
```

Definitions:

| Code | Meaning |
|---|---|
| `draft` | Editable invoice or credit-note draft without a final legal number. |
| `finalised` | Numbered and financially locked, but not recorded as sent. |
| `sent` | Finalised document has been delivered or marked as delivered. |
| `part_paid` | Valid non-reversed payments cover part of the outstanding amount. |
| `paid` | Outstanding amount is zero because of payments, excluding full credit. |
| `cancelled` | Draft was abandoned before finalisation. |
| `credited` | A finalised invoice has been fully offset by finalised credit notes. |

Rules:

- `overdue` is derived and must not be stored;
- only drafts may normally become `cancelled`;
- a finalised or sent invoice is corrected using a credit note, not cancellation;
- partial credit does not automatically use `credited`;
- payment creation and reversal recalculate `part_paid` or `paid`;
- a fully credited invoice uses `credited`, even if it had previously been sent or paid;
- finalised financial content remains immutable.

### Normal transitions

```text
draft      -> finalised, cancelled
finalised  -> sent, part_paid, paid, credited
sent       -> part_paid, paid, credited
part_paid  -> sent, paid, credited
paid       -> part_paid or sent after payment reversal; credited after full credit
credited   -> terminal except through an explicit corrective credit-note workflow
cancelled  -> terminal
```

The exact post-reversal status is recalculated from payment, credit and delivery data rather than hard-coded blindly.

## 6.3 Invoice line type codes

Column:

```text
invoice_lines.line_type_code
```

Allowed values:

```text
time
fixed_price
manual
expense_recharge
adjustment
credit
```

Definitions:

| Code | Meaning |
|---|---|
| `time` | Line produced from one or more time entries. |
| `fixed_price` | Agreed fixed-price work. |
| `manual` | General manually entered line. |
| `expense_recharge` | Recorded business expense recharged to a customer. |
| `adjustment` | Explicit positive or negative adjustment with a reason. |
| `credit` | Credit-note line reversing all or part of an original line. |

No default is approved. The creating workflow must choose the line type.

## 6.4 Discount type codes

Column:

```text
invoice_lines.discount_type_code
```

Allowed values:

```text
none
percentage
fixed_amount
```

Default:

```text
none
```

Whole-invoice discounts are not supported in the MVP.

## 6.5 Invoice sequence codes

Column:

```text
invoice_number_sequences.sequence_code
```

Allowed values:

```text
invoice
credit_note
```

Invoice and credit-note sequences remain independently configurable while each document number remains unique.

## 6.6 Payment method codes

Columns:

```text
invoice_payments.payment_method_code
expenses.payment_method_code
```

Allowed values:

```text
bank_transfer
cash
debit_card
credit_card
direct_debit
standing_order
cheque
other
```

No default is approved.

A reversal is represented by the payment reversal fields and audit history, not by a separate payment-method code.

---

# 7. Financial-account codes

## 7.1 Financial account type codes

Table:

```text
financial_account_types
```

The account types are data-driven lookup records rather than a check-constrained column on `financial_accounts`.

Approved seeded values:

### Assets

```text
current_account
savings_account
regular_saver
fixed_rate_saver
cash_isa
stocks_shares_isa
lifetime_isa
investment_account
pension
cash
other_asset
```

### Liabilities

```text
credit_card
overdraft
personal_loan
student_loan
mortgage
other_liability
```

New account types require a migration or controlled lookup-data change. Existing codes must not be renamed after use.

## 7.2 Classification codes

Column:

```text
financial_account_types.classification_code
```

Allowed values:

```text
asset
liability
```

Net worth uses the documented asset-minus-liability rules.

## 7.3 Account scope codes

Column:

```text
financial_accounts.account_scope_code
```

Allowed values:

```text
business
personal
```

No default is approved. Scope must be explicitly selected.

Business and personal data must remain separated by default in lists, dashboards, exports and reports.

## 7.4 Financial account status codes

Column:

```text
financial_accounts.account_status_code
```

Allowed values:

```text
open
dormant
restricted
closed
```

Default:

```text
open
```

Definitions:

| Code | Meaning |
|---|---|
| `open` | Account is open and normally usable. |
| `dormant` | Account remains open but is inactive or dormant. |
| `restricted` | Account remains open but access or use is restricted. |
| `closed` | Account has been closed. |

Rules:

- `closed` requires `closed_date`;
- non-closed accounts normally have `closed_date = NULL`;
- maturity is derived from `maturity_date` and is not an account status;
- hiding is controlled by `is_hidden`;
- archiving is separate and is used only to remove historical/closed accounts from normal lists;
- account balances and snapshots are never deleted when an account closes.

## 7.5 Interest-rate type codes

Column:

```text
financial_accounts.interest_rate_type_code
```

Allowed values:

```text
variable
fixed
tracker
promotional
```

`NULL` means not applicable or not recorded.

A promotional end date may be recorded separately.

## 7.6 Tax-wrapper codes

Column:

```text
financial_accounts.tax_wrapper_code
```

Allowed values:

```text
cash_isa
stocks_shares_isa
lifetime_isa
pension
```

`NULL` means the account is not recorded as a tax wrapper.

The selected code must agree with the account type.

## 7.7 Balance snapshot source codes

Column:

```text
financial_account_balance_snapshots.snapshot_source_code
```

Allowed values:

```text
manual
statement
import
system
```

Default:

```text
manual
```

Definitions:

| Code | Meaning |
|---|---|
| `manual` | User directly entered or updated the balance. |
| `statement` | User entered the balance from a bank or provider statement. |
| `import` | Created by a future approved import process. |
| `system` | Created by a controlled internal workflow, such as account creation. |

`import` is reserved for later scope and must not be emitted until an approved import feature exists.

## 7.8 Account-application status codes

Column:

```text
financial_account_applications.application_status_code
```

Allowed values:

```text
considering
planned
applied
identity_check
awaiting_information
approved
declined
withdrawn
opened
completed
```

Default:

```text
considering
```

Definitions:

| Code | Meaning |
|---|---|
| `considering` | Product is being considered with no commitment. |
| `planned` | User intends to apply. |
| `applied` | Application has been submitted. |
| `identity_check` | Provider is carrying out identity or eligibility checks. |
| `awaiting_information` | Further information or action is required. |
| `approved` | Provider has approved the application but account opening is not complete. |
| `declined` | Provider declined the application. |
| `withdrawn` | User withdrew the application. |
| `opened` | Resulting financial account has been created and linked. |
| `completed` | No further application follow-up is required. |

### Normal transitions

```text
considering          -> planned, applied, withdrawn
planned              -> considering, applied, withdrawn
applied              -> identity_check, awaiting_information, approved, declined, withdrawn
identity_check       -> awaiting_information, approved, declined, withdrawn
awaiting_information -> identity_check, approved, declined, withdrawn
approved             -> opened, withdrawn
opened               -> completed
declined             -> completed
withdrawn            -> completed
completed            -> terminal
```

When moving to `opened`, create and link the resulting account in one transaction.

## 7.9 Application channel codes

Column:

```text
financial_account_applications.application_channel_code
```

Allowed values:

```text
online
telephone
branch
post
other
```

`NULL` means not recorded.

## 7.10 Contribution type codes

Column:

```text
financial_account_contributions.contribution_type_code
```

Allowed values:

```text
personal_contribution
employer_contribution
government_bonus
tax_relief
transfer_in
other
```

Contribution records are informational and do not automatically change account balances.

---

# 8. Security, settings, backup and audit codes

## 8.1 User role codes

Column:

```text
users.role_code
```

Allowed MVP value:

```text
administrator
```

Default:

```text
administrator
```

Additional roles are not added until multiple real users and permissions are approved.

## 8.2 Application-setting value type codes

Column:

```text
application_settings.value_type_code
```

Allowed values:

```text
string
integer
decimal
boolean
date
datetime
json
```

The settings service must parse and validate the stored value according to the code.

## 8.3 Backup type codes

Column:

```text
backup_records.backup_type_code
```

Allowed values:

```text
automatic_daily
manual
pre_migration
pre_restore
```

## 8.4 Backup status codes

Column:

```text
backup_records.status_code
```

Allowed values:

```text
in_progress
completed
failed
```

Verification and restore history are represented by their timestamps and audit records rather than overloading backup status.

## 8.5 Audit entity type codes

Column:

```text
audit_records.entity_type_code
```

Approved initial values:

```text
user
application_setting
customer
customer_contact
customer_address
job
time_entry
task
invoice
invoice_payment
expense
financial_account
financial_account_application
attachment
backup
system
```

This is an extensible C#-controlled list. New modules add a new explicit constant and tests.

## 8.6 Audit action codes

Column:

```text
audit_records.action_code
```

Approved initial values:

```text
created
updated
archived
restored
status_changed
corrected
finalised
sent
payment_recorded
payment_reversed
credit_note_created
balance_updated
application_converted
backup_started
backup_completed
backup_failed
backup_verified
restore_started
restore_completed
restore_failed
login_succeeded
login_failed
account_locked
password_changed
recovery_code_used
```

Audit action codes are centrally controlled but not database check-constrained because the list expands as modules are implemented.

---

# 9. Standards-based codes

## 9.1 Currency codes

Columns include:

```text
financial_accounts.currency_code
invoices.currency_code
```

Use uppercase ISO 4217 currency codes.

MVP default:

```text
GBP
```

Do not create a closed database check that prevents future legitimate currencies.

## 9.2 Country codes

Columns include:

```text
customer_addresses.country_code
invoices.bill_to_country_code
```

Use uppercase ISO 3166-1 alpha-2 codes.

MVP default:

```text
GB
```

---

# 10. Approved database defaults

The current MariaDB bootstrap schema was reviewed against the approved code sets.

The following defaults are approved:

| Table and column | Default |
|---|---|
| `users.role_code` | `administrator` |
| `users.is_active` | `1` |
| `customers.is_active` | `1` |
| `customer_addresses.country_code` | `GB` |
| `jobs.status_code` | `planned` |
| `jobs.priority_code` | `normal` |
| `active_timers.is_billable` | `1` |
| `time_entries.is_billable` | `1` |
| `time_entries.rounding_rule_code` | `none` |
| `tasks.status_code` | `not_started` |
| `tasks.priority_code` | `normal` |
| `financial_accounts.currency_code` | `GBP` |
| `financial_accounts.account_status_code` | `open` |
| `financial_account_balance_snapshots.snapshot_source_code` | `manual` |
| `financial_account_applications.application_status_code` | `considering` |
| `invoices.invoice_type_code` | `invoice` |
| `invoices.status_code` | `draft` |
| `invoices.currency_code` | `GBP` |
| `invoice_lines.discount_type_code` | `none` |

The following intentionally have no approved database default and must be supplied explicitly by the creating workflow:

```text
jobs.charging_type_code
time_entries.entry_method_code
financial_accounts.account_scope_code
invoice_lines.line_type_code
invoice_payments.payment_method_code
expenses.payment_method_code
financial_account_contributions.contribution_type_code
```

No conflicting default spelling was found in the reviewed bootstrap schema.

---

# 11. Required MariaDB constraint work

The current schema already check-constrains some values, including financial account classification and account scope.

The initial migration set should add or confirm `CHECK` constraints for all other closed sets documented here, including:

```text
users.role_code
application_settings.value_type_code
backup_records.backup_type_code
backup_records.status_code
customers.default_vat_treatment_code
customers.invoice_delivery_code
customer_addresses.address_type_code
jobs.status_code
jobs.priority_code
jobs.charging_type_code
time_entries.entry_method_code
time_entries.rounding_rule_code
tasks.status_code
tasks.priority_code
financial_accounts.account_status_code
financial_accounts.interest_rate_type_code
financial_accounts.tax_wrapper_code
financial_account_balance_snapshots.snapshot_source_code
financial_account_applications.application_status_code
financial_account_applications.application_channel_code
financial_account_contributions.contribution_type_code
invoice_number_sequences.sequence_code
invoices.invoice_type_code
invoices.status_code
invoice_lines.line_type_code
invoice_lines.discount_type_code
invoice_payments.payment_method_code
expenses.payment_method_code
```

Nullable code columns must permit `NULL` in addition to their listed values.

Constraint names should use:

```text
chk_<table>_<column>
```

Before adding a constraint to an existing database:

1. query distinct existing values;
2. correct or migrate invalid values;
3. add the constraint through FluentMigrator;
4. add an integration test proving an invalid value is rejected.

---

# 12. Implementation rules

1. WinForms controls display user-friendly labels, not raw database codes.
2. UI values must map to shared Core constants.
3. Repositories persist only codes already validated by the application service.
4. Repositories must still handle database constraint failures safely.
5. Status transitions belong in application services, not forms or repositories.
6. Status changes that affect financial, security or correction history create audit records.
7. `overdue` remains a derived invoice state.
8. Archive state remains separate from workflow status.
9. Closed or completed records remain historically navigable.
10. Unknown persisted values must cause a clear validation/error state; they must not silently map to a default.
11. A new code requires tests for validation, transitions and database persistence.
12. Codes are never localised in the database. Only display labels are localised or formatted.

---

# 13. Verification checklist

- [x] Customer active/archive behaviour is defined.
- [x] Job statuses are defined.
- [x] Job priorities are defined.
- [x] Charging types are defined.
- [x] Task statuses are defined.
- [x] Invoice types are defined.
- [x] Invoice statuses are defined.
- [x] Invoice line types are defined.
- [x] Financial account classifications are defined.
- [x] Account scopes are defined.
- [x] Financial account statuses are defined.
- [x] Time-entry methods are defined.
- [x] Time-rounding rules are defined.
- [x] Account-application statuses are defined.
- [x] C# representation is decided.
- [x] C# and MariaDB validation responsibilities are decided.
- [x] Current schema defaults use approved values.
- [x] Additional schema code columns are documented.
- [x] No conflicting spellings were found in the reviewed bootstrap schema.
- [!] The exact storage representation for `none` time rounding must be corrected or formally resolved during P1-03 before the schema baseline is approved.
- [ ] Closed-set MariaDB checks must be implemented or confirmed through the initial FluentMigrator migrations.
- [ ] C# code constants and transition tests must be implemented during the relevant development phases.

---

# 14. P1-02 result

**Workflow and code-value decision:** Approved.

The allowed values, defaults, representation strategy and validation strategy are now defined.

P1-02 documentation is complete. The noted time-duration storage issue is a schema follow-up for P1-03 and the migration baseline work; it does not leave the meaning of the `none` code undefined.
