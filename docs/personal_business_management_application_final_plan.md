# Personal Business Management Application — Final Development Plan

> **Document status:** Baseline source of truth  
> **Version:** 1.0  
> **Last updated:** 27 July 2026  
> **Primary developer:** Charlie Cook  
> **Intended implementation:** C# WinForms, .NET 10 LTS, local MariaDB, MySqlConnector, Dapper, FluentMigrator and QuestPDF  
> **Default currency and locale:** GBP (`en-GB`)  
> **Primary theme:** Dark  
> **Database naming:** Lowercase `snake_case`, with `record_id` primary keys

---

## Table of contents

1. [Purpose of this document](#1-purpose-of-this-document)
2. [How to use this plan](#2-how-to-use-this-plan)
3. [Executive product summary](#3-executive-product-summary)
4. [Non-negotiable architecture decisions](#4-non-negotiable-architecture-decisions)
5. [Recommended technology stack](#5-recommended-technology-stack)
6. [Product scope classification](#6-product-scope-classification)
7. [Application navigation and screen hierarchy](#7-application-navigation-and-screen-hierarchy)
8. [Dark-theme design system](#8-dark-theme-design-system)
9. [Customer-management requirements](#9-customer-management-requirements)
10. [Job-management requirements](#10-job-management-requirements)
11. [Time-tracking design](#11-time-tracking-design)
12. [Task-management design](#12-task-management-design)
13. [Invoice and credit-note design](#13-invoice-and-credit-note-design)
14. [Business-finance design](#14-business-finance-design)
15. [Personal-finance design](#15-personal-finance-design)
16. [Database conventions](#16-database-conventions)
17. [Proposed database schema](#17-proposed-database-schema)
18. [Initial migration order](#18-initial-migration-order)
19. [Application architecture](#19-application-architecture)
20. [Security and login](#20-security-and-login)
21. [Backup, restore and disaster recovery](#21-backup-restore-and-disaster-recovery)
22. [Search, filtering and large-list performance](#22-search-filtering-and-large-list-performance)
23. [Reporting requirements](#23-reporting-requirements)
24. [Validation and reliability rules](#24-validation-and-reliability-rules)
25. [Testing strategy](#25-testing-strategy)
26. [Development roadmap](#26-development-roadmap)
27. [Remote-access migration path](#27-remote-access-migration-path)
28. [Coding standards for Codex](#28-coding-standards-for-codex)
29. [Definition of done](#29-definition-of-done)
30. [Recommended Codex prompt template](#30-recommended-codex-prompt-template)
31. [Key risks and mitigations](#31-key-risks-and-mitigations)
32. [Final recommended first version](#32-final-recommended-first-version)
33. [Decision log](#33-decision-log)
34. [Change log](#34-change-log)
35. [Glossary](#35-glossary)
36. [Version-sensitive official references](#36-version-sensitive-official-references)

---

## 1. Purpose of this document

This document is the main development specification for a personal business-management application that will be built in stages using C# WinForms, MariaDB, Visual Studio, ChatGPT and Codex.

It is intended to be readable by:

- The application owner and developer.
- ChatGPT when discussing architecture or future changes.
- Codex when implementing a specific phase or feature.
- A future developer reviewing or maintaining the system.

This is not a request to generate the entire application in one operation. It defines the architecture, modules, data model, rules, interfaces, security expectations, roadmap and completion criteria needed to build the application without creating an unmaintainable collection of WinForms forms and SQL queries.

The plan is deliberately detailed because it will be reused as a long-term source of truth. Future edits should update this file rather than creating conflicting requirements elsewhere.

---

## 2. How to use this plan

### 2.1 Source-of-truth rule

When this plan conflicts with an older prompt, prototype, code comment or database script, this plan takes precedence unless a later approved version explicitly replaces it.

### 2.2 Future-edit procedure

When a requirement changes:

1. Update the relevant section.
2. Update the version number.
3. Add a dated entry to the change log.
4. Identify any affected tables, services, screens, tests and migrations.
5. Do not edit an already-released database migration. Add a new migration.
6. If code already exists, add a compatibility or data-migration plan.

Recommended versioning:

- `1.0` — initial approved baseline.
- `1.1` — requirements or design additions that do not fundamentally change the architecture.
- `2.0` — substantial architecture or product-scope change.

### 2.3 Instructions for ChatGPT and Codex

Before proposing or implementing a feature:

1. Read the relevant module section.
2. Read the architecture, database, security and testing sections.
3. Identify dependencies and affected tables.
4. Preserve all non-negotiable design rules.
5. Make the smallest coherent change that satisfies the requirement.
6. Add or update tests.
7. Update migrations when the schema changes.
8. Do not silently invent business rules.
9. Document assumptions in the change summary.
10. Keep the application runnable after each completed development slice.

### 2.4 Required implementation output from Codex

For a normal implementation task, Codex should provide:

- A concise summary of the implemented change.
- Files added or changed.
- Database migrations added.
- Tests added or updated.
- Manual test steps.
- Known limitations.
- Any plan sections that should be updated.

---

## 3. Executive product summary

The application is a single-user Windows desktop system that combines three related but clearly separated areas.

### 3.1 Business operations

- Customers.
- Customer contacts and addresses.
- Jobs.
- Job tasks.
- Time tracking.
- Job attachments.
- Activity history.

### 3.2 Business finance

- Invoices.
- Credit notes.
- Invoice PDFs.
- Payments and part payments.
- Expenses.
- Business financial accounts.
- Revenue, expense and profit reporting.
- Outstanding-invoice reporting.
- Configurable VAT estimates.
- Configurable tax-reserve estimates.

### 3.3 Personal finance

- Current accounts.
- Savings accounts.
- Regular savers.
- Fixed-rate savings accounts.
- Cash ISAs.
- Stocks and Shares ISAs.
- Lifetime ISAs.
- General investment accounts.
- Pensions, where the user wants to record them.
- Credit cards.
- Loans and other liabilities.
- Account balance history.
- Personal assets and liabilities.
- Estimated net worth.
- Applications for new financial accounts.
- ISA and savings contributions.
- Account maturity and promotional-rate dates.

Business and personal finance share infrastructure but must not be mixed in normal reports.

---

## 4. Non-negotiable architecture decisions

The following are baseline decisions unless a future approved plan changes them.

1. The desktop client uses **C# WinForms**.
2. The application targets **.NET 10 LTS** and stays current with supported patch releases.
3. The database is **MariaDB**, initially running on the same Windows computer.
4. The validated production baseline should use a maintained MariaDB LTS release. At the time of this document, MariaDB 11.8 is the preferred baseline. An existing XAMPP MariaDB instance may be used temporarily for development, but must not become an unreviewed production dependency.
5. All application tables use the **InnoDB** storage engine.
6. The database uses **`utf8mb4`**.
7. Database tables and columns use lowercase **`snake_case`**.
8. Most primary keys are named **`record_id`** and use `BIGINT UNSIGNED AUTO_INCREMENT`.
9. C# classes and properties use normal .NET naming conventions such as `CustomerService` and `RecordId`.
10. Forms and `UserControl` pages do not contain SQL.
11. Forms call application services.
12. Application services enforce workflows and business rules.
13. Infrastructure repositories and query classes communicate with MariaDB.
14. All SQL parameters are parameterised. User data is never concatenated into SQL.
15. Financial calculations use C# `decimal`, never `float` or `double`.
16. Finalised financial records are not silently edited.
17. Important financial changes create audit records.
18. Hard deletion is avoided for business and financial history.
19. Only one active timer may exist per application user.
20. A time entry cannot be invoiced twice.
21. MariaDB is not exposed directly to the public internet.
22. Future remote access uses an authenticated API and/or private VPN architecture.
23. The application is dark themed from the first development phase.
24. Personal-finance tracking does not store online-banking passwords, PINs, card security codes or authentication secrets.
25. The application provides management and planning estimates, not professional tax, accounting or regulated financial advice.

---

## 5. Recommended technology stack

### 5.1 Desktop application

- C#.
- .NET 10 LTS.
- WinForms.
- `Microsoft.Extensions.DependencyInjection`.
- `Microsoft.Extensions.Logging`.
- Reusable `UserControl` pages inside one main shell form.

### 5.2 Database and data access

- MariaDB 11.8 LTS or a later approved maintained LTS version.
- InnoDB.
- MySqlConnector.
- Dapper.
- FluentMigrator.

### 5.3 Reporting

- QuestPDF for invoice and report PDFs.
- CSV export implemented directly or through a small, reviewed CSV library.

### 5.4 Testing

- xUnit.
- FluentAssertions, where useful.
- Testcontainers for .NET or a dedicated test MariaDB instance for integration tests.
- A separate test database that is never the production database.

### 5.5 Logging

Use `Microsoft.Extensions.Logging` with a rolling file provider such as Serilog if required. Logs must not contain passwords, connection strings, full banking details or unnecessary personal data.

### 5.6 Why Dapper is recommended

Dapper is recommended because this application benefits from explicit relational queries, predictable SQL, efficient paging and clear control over transactions.

Dapper should not be used as justification for placing SQL anywhere in the UI. SQL remains in Infrastructure repositories and query classes.

### 5.7 Why MySqlConnector is recommended

MySqlConnector provides an asynchronous ADO.NET driver for MariaDB/MySQL-compatible servers. Use short-lived connections obtained through a connection factory. Do not share one open connection across forms or threads.

### 5.8 Why FluentMigrator is recommended

FluentMigrator provides versioned, source-controlled migrations without making the application dependent on an ORM. Migrations are applied in order and recorded in the database.

### 5.9 QuestPDF licensing

QuestPDF currently uses a hybrid licence. Its Community Licence is available to individuals and qualifying organisations below its stated revenue threshold, subject to its current terms. The licence must be checked again before production distribution or use by an organisation that may no longer qualify.

---

## 6. Product scope classification

## 6.1 Essential MVP features

### Platform and reliability

- Secure local application login.
- MariaDB connection management.
- Database migrations.
- Automatic backup.
- Manual backup.
- Tested restore process.
- Audit history.
- Error logging.
- Dark application shell.
- Data validation.
- Soft deletion and archiving.
- CSV export for important lists.

### Business operations

- Customers.
- Contacts.
- Addresses.
- Jobs.
- Job status and priority.
- Tasks.
- Persistent active timer.
- Manual time records.
- Billable and non-billable time.
- Job attachments.

### Business finance

- Draft invoices.
- Finalised invoices.
- Credit notes.
- Time-based lines.
- Fixed and manual lines.
- VAT configuration.
- Invoice PDF generation.
- Payments and part payments.
- Expenses and expense categories.
- Business accounts.
- Revenue, expense and profit estimates.
- Outstanding-invoice reporting.

### Personal finance

- Financial account types.
- Personal accounts.
- Business/personal scope separation.
- Assets and liabilities.
- Manual balance updates.
- Balance snapshots.
- Estimated net worth.
- Financial account applications.
- ISA and savings contributions.
- Rate, maturity and promotional-end dates.

## 6.2 Useful later

- Quotes and estimates.
- Quote-to-job conversion.
- Recurring tasks.
- Recurring jobs.
- Recurring invoices.
- Invoice emailing.
- Overdue-invoice email reminders.
- Personal budgets.
- Savings goals.
- Personal transaction tracking.
- Bank-statement CSV import.
- Transfer tracking between accounts.
- Investment holdings and allocation.
- Account maturity notifications.
- System-tray timer controls.
- Saved filters.
- Multiple application users.
- User roles and permissions.
- Hosted API.
- Remote access.
- Optional web or mobile client.

## 6.3 Optional

- Light theme.
- User-selectable accent colour.
- Custom dashboard layout.
- Multiple invoice designs.
- Calendar task view.
- Favourite jobs and customers.
- Outlook or Google Calendar integration.
- Windows Hello unlocking.
- Custom report designer.
- Provider logos.

## 6.4 Avoid in the first version

- Full double-entry bookkeeping.
- General ledger and journal entries.
- Payroll.
- Bank reconciliation.
- Open Banking connections.
- Automatic banking login.
- Automatic tax return submission.
- Automatic VAT submission.
- Investment trading.
- Live market-price feeds.
- Customer portal.
- Multi-company SaaS architecture.
- Stock control.
- Purchase-order system.
- Workflow designer.
- Plug-in system.
- Microservices.
- Event sourcing.
- Message queues.
- Artificial-intelligence financial categorisation.

---

## 7. Application navigation and screen hierarchy

## 7.1 Logical record hierarchy

```text
Login

Dashboard
├── Business overview
│   ├── Active timer
│   ├── Tasks due
│   ├── Outstanding invoices
│   ├── Monthly invoiced revenue
│   ├── Monthly received revenue
│   └── Monthly expenses
│
├── Personal-finance overview
│   ├── Total assets
│   ├── Total liabilities
│   ├── Estimated net worth
│   ├── Savings and ISA total
│   ├── Balance updates due
│   └── Account-application follow-ups
│
├── Customers
│   └── Customer details
│       ├── Contacts
│       ├── Addresses
│       ├── Jobs
│       │   └── Job details
│       │       ├── Time entries
│       │       ├── Active timer
│       │       ├── Tasks
│       │       ├── Attachments
│       │       └── Invoice history
│       ├── Invoices
│       ├── Attachments
│       ├── Notes
│       └── Activity
│
├── Jobs
│   └── Job details
│       ├── Time entries
│       ├── Tasks
│       ├── Attachments
│       └── Invoices
│
├── Time
│   ├── Active timer
│   ├── Time-entry list
│   └── Manual time entry
│
├── Tasks
│   ├── Overdue
│   ├── Today
│   ├── Upcoming
│   ├── No due date
│   └── Completed
│
├── Invoices
│   ├── Draft
│   ├── Finalised
│   ├── Sent
│   ├── Outstanding
│   ├── Paid
│   ├── Cancelled
│   ├── Credit notes
│   └── Invoice editor/viewer
│
├── Business finance
│   ├── Payments
│   ├── Expenses
│   ├── Business accounts
│   ├── Revenue reports
│   ├── Profit estimates
│   ├── VAT estimate
│   └── Tax reserve
│
├── Personal finance
│   ├── Accounts
│   │   └── Account details
│   │       ├── Overview
│   │       ├── Balance history
│   │       ├── Contributions
│   │       ├── Attachments
│   │       ├── Notes
│   │       └── Activity
│   ├── Applications
│   ├── Assets and liabilities
│   ├── Net worth
│   └── Personal reports
│
├── Audit history
├── Backups
└── Settings
```

## 7.2 Permanent sidebar

The sidebar should remain concise:

```text
Dashboard

WORK
Customers
Jobs
Time
Tasks

BUSINESS FINANCE
Invoices
Expenses
Business Reports

PERSONAL FINANCE
Accounts
Applications
Personal Reports

SYSTEM
Audit History
Backups
Settings
```

Related records should be opened from detail pages and tabs rather than creating deeply nested permanent menus.

## 7.3 Main shell

The application should use one main shell form with:

- Collapsible sidebar.
- Top header.
- Current page title.
- Breadcrumbs.
- Main content panel.
- Persistent active-timer strip.
- Current-user menu.
- Backup status indicator.
- Non-blocking notification area.
- Loading overlay for longer actions.

Main pages should be reusable `UserControl` classes loaded into the content panel.

Dialog forms should be reserved for focused operations such as:

- Add/edit customer.
- Add/edit contact.
- Add/edit job.
- Add/edit task.
- Correct time entry.
- Record payment.
- Update account balance.
- Confirm invoice finalisation.
- Restore backup.

---

## 8. Dark-theme design system

## 8.1 Theme requirement

Dark mode is the primary and required appearance. Do not build unstyled standard WinForms screens and plan to theme them later.

## 8.2 Suggested palette

```text
application_background    #111318
sidebar_background        #171a20
panel_background          #1d2128
raised_panel              #242932
input_background           #191d23
border_colour              #343b46
primary_text               #f1f3f5
secondary_text             #aab1bb
muted_text                 #747d89
accent                     #7c6cf2
accent_hover               #9184f7
success                    #46b981
warning                    #d6a64a
danger                     #dc5c68
selection_background       #302b55
```

Exact colours may be adjusted, but all controls must consume shared theme tokens.

## 8.3 Typography

- Prefer Segoe UI or Segoe UI Variable.
- Body: approximately 10–11 pt.
- Form section heading: approximately 12–14 pt.
- Page heading: approximately 18–22 pt.
- Use semibold weight for headings rather than excessive font size.

## 8.4 Spacing

Use an 8-pixel spacing system:

- 4 px: very small internal separation.
- 8 px: standard related-control separation.
- 16 px: section separation.
- 24–32 px: major content separation.

## 8.5 Reusable controls

Create reusable controls or styling helpers for:

- `DarkButton`.
- `DarkTextBox`.
- `DarkComboBox`.
- `DarkDateTimePicker`.
- `DarkDataGridView`.
- `DarkTabControl`.
- `PageHeader`.
- `FilterBar`.
- `SummaryCard`.
- `StatusBadge`.
- `EmptyStatePanel`.
- `LoadingOverlay`.
- `ValidationMessage`.
- `ConfirmDialog`.
- `CurrencyTextBox`.
- `DurationTextBox`.

## 8.6 Theme infrastructure

Use shared classes such as:

```text
ThemePalette
ThemeManager
ControlStyler
UiSpacing
UiFonts
```

Do not hard-code colour values throughout individual forms.

## 8.7 DataGridView standards

- Dark header and row backgrounds.
- Subtle alternating rows.
- Restrained gridlines.
- Visible focus state.
- Clear selected-row contrast.
- Consistent date, duration and GBP formatting.
- Status badges where practical.
- Double buffering to reduce flicker.
- Explicit empty state rather than an unexplained blank grid.
- Paging controls below the grid.

## 8.8 Accessibility and usability

- Maintain readable contrast.
- Do not rely only on colour to communicate status.
- Use text and icons together.
- Provide visible focus cues.
- Support keyboard navigation.
- Ensure controls remain usable with Windows display scaling.
- Use `AutoScaleMode.Dpi` or an equivalent deliberate DPI strategy.

---

## 9. Customer-management requirements

### CUST-001 — Customer identity

A customer may represent a company, organisation or individual.

Required or supported fields:

- Company name or display name.
- Active/archived state.
- Default hourly rate.
- Default payment terms.
- Default VAT treatment.
- Invoice delivery preference.
- Notes.

### CUST-002 — Contacts

A customer may have multiple contacts.

Contact fields:

- Contact name.
- Job title.
- Email address.
- Phone number.
- Mobile number.
- Primary-contact flag.
- Notes.
- Archived state.

Only one active primary contact should normally exist. Because MariaDB does not provide a simple partial unique index for this rule, enforce it in a transaction within the customer service.

### CUST-003 — Addresses

A customer may have multiple addresses:

- Billing.
- Service/site.
- Registered.
- Other.

Support one default active address per address type through application validation.

### CUST-004 — Customer details page

Tabs:

- Overview.
- Contacts.
- Addresses.
- Jobs.
- Invoices.
- Attachments.
- Notes.
- Activity.

### CUST-005 — Archiving

Archiving a customer:

- Removes it from normal active lists.
- Does not delete jobs, invoices, payments or time records.
- Prevents new jobs unless restored or explicitly overridden.
- Keeps historical navigation available.

### CUST-006 — Search

Search by:

- Company/display name.
- Contact name.
- Email.
- Phone.
- Postcode.

### Customer acceptance criteria

- A customer can be created with at least one contact and one address in one transaction.
- Duplicate saves caused by repeated button clicks are prevented.
- Archived customers are excluded by default.
- Historical invoices remain unchanged when customer details change.
- Forms contain no SQL.

---

## 10. Job-management requirements

### JOB-001 — Core fields

- Customer.
- Unique job number.
- Title.
- Description.
- Status.
- Priority.
- Charging type.
- Start date.
- Due date.
- Estimated hours.
- Agreed hourly rate.
- Fixed price.
- Notes.
- Completed timestamp.
- Archived timestamp.

### JOB-002 — Statuses

Initial fixed codes:

```text
planned
active
on_hold
completed
cancelled
```

Archive is separate from workflow status.

### JOB-003 — Priorities

```text
low
normal
high
urgent
```

### JOB-004 — Charging type

```text
hourly
fixed_price
mixed
non_billable
```

`mixed` allows time-based and fixed/manual invoice items.

### JOB-005 — Rate precedence

```text
job.agreed_hourly_rate
    ↓ when null
customer.default_hourly_rate
    ↓ when null
application setting default_hourly_rate
```

The effective rate should be shown to the user before time is invoiced.

The actual billed rate is copied to the final invoice line/link and never recalculated from current defaults.

### JOB-006 — Job details page

Tabs or sections:

- Overview.
- Time.
- Tasks.
- Attachments.
- Invoices.
- Notes.
- Activity.

### JOB-007 — Completion and archiving

Completing a job:

- Sets the status to `completed`.
- Records `completed_utc`.
- Stops or prevents new active timers unless the user reopens the job.
- Does not automatically invoice time.

Archiving a job:

- Requires no active timer.
- Does not remove historical data.
- Excludes it from normal lists.

### Job acceptance criteria

- Job numbers are unique.
- Due date cannot be earlier than start date.
- Fixed-price jobs require a fixed price before invoicing.
- Hourly billing resolves a valid effective rate before invoice finalisation.
- Related time, tasks and invoices remain navigable.

---

## 11. Time-tracking design

## 11.1 Core design

Use a separate `active_timers` table for running timers and a `time_entries` table for completed entries.

This makes the one-active-timer constraint simple and reliable:

```text
UNIQUE(active_timers.user_id)
```

## 11.2 Starting a timer

Within one transaction:

1. Validate the authenticated user.
2. Validate that the job exists and is available for time recording.
3. Check for an existing active timer.
4. If one exists, require the user to continue it, stop and switch, or cancel.
5. Insert the new active timer.
6. Commit.
7. Refresh the persistent timer strip.

The database unique constraint is the final protection against duplicate active timers.

## 11.3 Stopping a timer

Within one transaction:

1. Load and lock the active-timer row.
2. Capture `end_utc` through the central clock service.
3. Validate `end_utc > start_utc`.
4. Calculate raw seconds.
5. Apply the selected rounding rule.
6. Insert the completed `time_entries` row.
7. Delete the active-timer row.
8. Create audit information where required.
9. Commit.

A failure must roll back both the insertion and deletion.

## 11.4 Persistent timer state

The visible timer is reconstructed from MariaDB after login. The application must not depend on an in-memory counter for accuracy.

The UI timer may refresh once per second, but elapsed time is always calculated as:

```text
current_utc - active_timer.start_utc
```

## 11.5 Forgotten timers

Add a configurable warning threshold.

When a timer exceeds it, allow:

- Continue.
- Stop now.
- Enter actual end time.
- Cancel timer with a reason.

Do not silently alter the record.

## 11.6 Manual time entries

Allow:

- Start and end timestamps.
- Date plus duration.

Require:

- Job.
- Description of work.
- Billable flag.
- Positive duration.

Set:

```text
entry_method_code = 'manual'
```

## 11.7 Time correction

Editing start, end, duration, job or billable status requires:

- A reason.
- Previous values in the audit record.
- New values in the audit record.
- Recalculated raw and rounded duration.
- Optimistic concurrency validation.

A time entry attached to a finalised invoice cannot be directly edited. Corrections require a credit/replacement process or a clearly defined administrative correction workflow.

## 11.8 Billable and non-billable time

Each entry stores `is_billable`.

Non-billable records:

- Appear in time and utilisation reports.
- Never appear in normal invoice selection.
- Can only be changed to billable through an audited edit.

## 11.9 Rounding

Store:

- `raw_duration_seconds`.
- `rounded_duration_minutes`.
- `rounding_rule_code`.

Initial rules:

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

When a time entry is invoiced, store the exact billed duration and rate in the invoice-time link.

## 11.10 Duplicate invoicing

The `invoice_time_entries` table must have:

```text
UNIQUE(time_entry_id)
```

Draft invoices reserve linked time entries. Cancelling or permanently removing an unfinalised draft may release the reservation. Finalised invoice links are retained.

### Time acceptance criteria

- One user cannot have two active timers.
- An active timer survives application and Windows restarts.
- Stopping a timer is atomic.
- Raw and rounded values remain available.
- Manual corrections are audited.
- A time entry cannot be included on two invoices.

---

## 12. Task-management design

### TASK-001 — Core fields

- Optional job.
- Title.
- Notes.
- Status.
- Priority.
- Due date.
- Completion timestamp.
- Archive timestamp.

A task with no job is a general business task.

### TASK-002 — Statuses

```text
not_started
in_progress
blocked
completed
cancelled
```

### TASK-003 — Views

- Overdue.
- Today.
- Upcoming.
- No due date.
- Completed.
- All.

### TASK-004 — Completion

Completing a task:

- Sets `status_code = 'completed'`.
- Sets `completed_utc`.

Reopening clears `completed_utc` and creates an audit event if the completion history matters.

### TASK-005 — Recurrence

Recurring tasks are later scope. When added, use recurrence definitions that generate individual task records. Do not repeatedly reset one task because that loses historical completion data.

### Task acceptance criteria

- Tasks may be job-specific or general.
- Overdue logic uses the user's local date.
- Completed tasks do not appear in active views by default.
- Related jobs can be opened directly.

---

## 13. Invoice and credit-note design

## 13.1 Invoice types

```text
invoice
credit_note
```

## 13.2 Invoice statuses

Stored statuses:

```text
draft
finalised
sent
part_paid
paid
cancelled
credited
```

`overdue` is normally derived rather than stored.

## 13.3 Draft invoices

Drafts may be created from:

- Customer.
- Job.
- Selected time entries.
- Date range.
- Manual invoice.

Drafts do not receive the final legal invoice number until finalisation.

## 13.4 Customer billing snapshot

At finalisation, copy the billing identity and address into invoice fields.

Later changes to the customer do not change historical invoices.

## 13.5 Invoice lines

Supported line types:

```text
time
fixed_price
manual
expense_recharge
adjustment
credit
```

Fields include:

- Description.
- Quantity.
- Unit rate.
- Net amount.
- VAT rate.
- VAT amount.
- Gross amount.
- Optional source job.
- Optional original invoice line for credit notes.

## 13.6 Discounts

To avoid ambiguous VAT allocation, the MVP should prefer line-level discounts.

Supported line discount forms:

```text
none
percentage
fixed_amount
```

Calculation order:

1. Calculate quantity × unit rate.
2. Apply line discount.
3. Round net amount to two decimal places.
4. Calculate VAT on the discounted net amount.
5. Round VAT to two decimal places.
6. Calculate gross amount.
7. Sum stored rounded line totals.

A whole-invoice discount across mixed VAT rates should wait until a tested allocation rule is designed.

## 13.7 VAT

VAT must be configurable rather than assumed.

Settings:

- Business VAT registered.
- VAT registration number.
- Default VAT rate.
- Prices entered inclusive or exclusive.
- Default customer VAT treatment.
- Line-specific VAT rate.
- Zero-rated or exempt reason where relevant.

The application must store the VAT rate and calculated amount used on each finalised line.

## 13.8 Financial rounding

Use C# `decimal`.

Use an explicit rounding method, documented and tested. A practical default is rounding monetary line values to two decimal places using `MidpointRounding.AwayFromZero`.

Do not depend on UI-formatted strings for calculations.

## 13.9 Invoice-number allocation

Use `invoice_number_sequences`.

Inside a transaction:

1. Select the sequence row using `FOR UPDATE`.
2. Read `next_number`.
3. Generate the formatted invoice number.
4. Increment the sequence.
5. Assign the number to the invoice.
6. Finalise the invoice.
7. Commit.

Never use:

```sql
SELECT MAX(invoice_number) + 1
```

The invoice table must also have a unique constraint on `invoice_number`.

## 13.10 Finalisation transaction

Invoice finalisation must:

1. Re-read and validate the draft.
2. Validate customer existence.
3. Validate at least one valid line.
4. Revalidate selected time entries.
5. Lock and allocate the invoice number.
6. Capture the billing snapshot.
7. Recalculate every line server-side/application-service-side.
8. Store net, VAT and gross totals.
9. Create time-entry links.
10. Set finalised status and timestamp.
11. Create audit records.
12. Commit.

Repeated finalise clicks must not create duplicate invoices.

## 13.11 PDF generation

Generate the PDF after the financial transaction commits.

Process:

1. Load an immutable invoice document model.
2. Generate bytes with QuestPDF.
3. Write a temporary file.
4. Calculate SHA-256.
5. Move to the final path atomically.
6. Store relative path, hash and generated timestamp.

If PDF generation fails:

- The invoice remains valid.
- The UI displays a PDF-generation error.
- The PDF can be regenerated from stored invoice data.

## 13.12 Payments

Payments store:

- Invoice.
- Date.
- Amount.
- Method.
- Reference.
- Receiving business account.
- Notes.
- Reversal information.

Payments are reversed, not physically deleted.

Invoice status calculation:

- No payment: finalised/sent.
- Payment below gross total: part paid.
- Payment equals total after credits: paid.
- Overpayment: require explicit confirmation and display separately.

## 13.13 Overdue calculation

```text
outstanding_amount > 0
AND due_date < current_local_date
AND status_code IN ('finalised', 'sent', 'part_paid')
```

Do not require a daily process to permanently change the status to overdue.

## 13.14 Credit notes

A credit note:

- Has its own unique number.
- References the original invoice.
- References original lines where applicable.
- Stores a reason.
- Does not reattach original time entries.
- Reduces the customer's outstanding balance.
- Cannot credit more than the available uncredited amount without an explicit authorised override.

### Invoice acceptance criteria

- Duplicate invoice numbers are impossible.
- Duplicate time invoicing is impossible.
- Finalisation is atomic.
- Stored totals match PDF totals.
- Historical customer information remains unchanged.
- Finalised lines are read-only.
- Credit notes preserve the original invoice.
- Payments and reversals reconcile with the outstanding balance.

---

## 14. Business-finance design

## 14.1 Revenue measures

### Invoiced revenue

```text
finalised invoice gross/net totals
minus finalised credit notes
grouped by invoice date
```

The user should be able to view gross or net revenue depending on the report.

### Received income

```text
non-reversed invoice payments
grouped by payment date
```

Do not treat a finalised invoice as received cash.

## 14.2 Expenses

Expense fields:

- Expense date.
- Supplier.
- Category.
- Description.
- Net amount.
- VAT amount.
- Gross amount.
- Business account paid from.
- Payment method.
- Reference.
- Estimated tax-deductible flag.
- Notes.
- Receipt attachment.
- Archived state.

`is_tax_deductible_estimate` is a planning aid, not a legal conclusion.

## 14.3 Profit estimates

```text
invoiced_profit_estimate = invoiced_revenue - recorded_business_expenses
```

```text
cash_profit_estimate = received_income - recorded_paid_expenses
```

Clearly label both as estimates.

## 14.4 Tax reserve

Allow a configurable reserve percentage:

```text
estimated_tax_reserve = max(estimated_profit, 0) × configured_percentage
```

Display a permanent notice:

> Planning estimate only. This is not a tax calculation or a replacement for professional accounting advice.

## 14.5 VAT estimate

When enabled, report:

- Output VAT from invoices.
- Input VAT recorded on expenses.
- Estimated net VAT position.
- Supporting records.

Do not present the result as a filed VAT return.

## 14.6 Outstanding-invoice ageing

- Not due.
- 1–30 days overdue.
- 31–60 days overdue.
- 61–90 days overdue.
- More than 90 days overdue.

## 14.7 Business accounts

Business current and savings accounts use the same `financial_accounts` model as personal accounts, but with:

```text
account_scope_code = 'business'
```

Payments and expenses may link to business accounts.

---

## 15. Personal-finance design

## 15.1 Purpose

The personal-finance module should answer:

- How much is held across personal accounts?
- How much is in current accounts, savings and ISAs?
- What liabilities are outstanding?
- What is the estimated net worth?
- How have balances changed?
- Which applications require action?
- Which rates, offers or maturity dates are approaching?

## 15.2 Account types

Initial seeded types:

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

Account types should remain data-driven through `financial_account_types`.

## 15.3 Balance semantics

Each account type has:

```text
classification_code = 'asset' or 'liability'
```

For asset accounts:

- Positive balance increases net worth.
- A negative current-account balance reduces net worth.

For liability accounts:

- Positive balance means an amount owed and is subtracted from net worth.
- A negative liability balance represents a credit and increases net worth.

Net-worth formula:

```text
sum(asset account balances) - sum(liability account balances)
```

## 15.4 Account fields

- Scope: business or personal.
- Provider.
- Account name.
- Account type.
- Last four reference digits.
- Currency.
- Status.
- Current balance.
- Available balance.
- Credit limit where relevant.
- Interest rate.
- Rate type.
- Introductory-rate end date.
- Fixed-rate end date.
- Maturity date.
- Opened date.
- Closed date.
- Tax-wrapper type.
- Notes.
- Last balance update.

Do not store full card numbers or login credentials.

## 15.5 Balance snapshots

Every balance update creates a snapshot rather than overwriting history only.

Within one transaction:

1. Validate account.
2. Insert snapshot.
3. Update current account balance.
4. Set last-updated timestamp.
5. Create audit record.
6. Commit.

Snapshot sources:

```text
manual
statement
import
system
```

## 15.6 Contributions

Contribution records are informational in the MVP.

They track:

- Account.
- Date.
- Amount.
- Contribution type.
- Tax year where relevant.
- Notes.

Contribution records do not automatically change net worth unless the corresponding balances are also updated. This avoids accidental double counting before transfer tracking exists.

## 15.7 ISA tracking

Track:

- ISA type.
- Provider.
- Current value.
- Contributions.
- Contribution date.
- Tax year.
- Manually recorded transfers.

Any allowance calculation depends on complete user-entered data and must be labelled an estimate.

Do not hard-code current ISA limits as permanent business logic. Use settings or future dated rules if allowance checking is added.

## 15.8 Account applications

Track applications for current accounts, savings products, ISAs, credit cards and other financial products.

Fields:

- Provider.
- Product name.
- Account type.
- Status.
- Date considered.
- Date applied.
- Decision date.
- Expected opening date.
- Next action date.
- Application reference.
- Advertised rate.
- Advertised bonus.
- Promotional end date.
- Channel.
- Notes.
- Linked opened account.

Statuses:

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

When an application becomes an account, create and link the account in one transaction.

## 15.9 Attachments and sensitive documents

Permitted examples:

- Product terms.
- Confirmation letters.
- Rate notices.
- Statements when the user deliberately chooses to store them.

Avoid storing identity documents such as passports or driving licences in the MVP. If later required, design an explicit encrypted-document solution and retention policy.

## 15.10 Personal-finance reports

- Account balances.
- Assets by account type.
- Liabilities by account type.
- Asset versus liability total.
- Estimated net worth.
- Net-worth history.
- Savings total.
- ISA total.
- Balance change by month.
- Applications by status.
- Upcoming maturity dates.
- Upcoming promotional-rate end dates.

### Personal-finance acceptance criteria

- Personal and business accounts are separated by default.
- Every balance update creates history.
- Net worth follows the documented formula.
- Applications can be converted into accounts.
- No online-banking credentials are stored.
- Contribution totals are not confused with account balances.

---

## 16. Database conventions

## 16.1 Database name

Recommended:

```text
personal_business_manager
```

## 16.2 Naming

Tables and columns use lowercase `snake_case`.

Examples:

```text
customers
customer_contacts
invoice_time_entries
financial_account_balance_snapshots
```

```text
record_id
customer_id
company_name
date_created_utc
version_no
```

## 16.3 Constraint names

Recommended prefixes:

```text
pk_   primary key
fk_   foreign key
uq_   unique constraint/index
idx_  non-unique index
chk_  check constraint
```

Examples:

```text
pk_customers
fk_jobs_customer_id
uq_invoices_invoice_number
idx_time_entries_job_start
chk_jobs_due_date
```

## 16.4 C# naming

C# remains PascalCase/camelCase:

```csharp
public sealed class CustomerDto
{
    public long RecordId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
}
```

Dapper queries may use explicit aliases:

```sql
SELECT
    record_id AS RecordId,
    company_name AS CompanyName
FROM customers;
```

A tested global underscore-name mapping convention may also be enabled, but complex projections should remain explicit.

## 16.5 Primary keys

Default:

```sql
record_id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT
```

## 16.6 Foreign keys

Foreign-key columns use the related entity name:

```text
customer_id
job_id
invoice_id
user_id
financial_account_id
```

Use `ON DELETE RESTRICT` for important business and financial relationships unless a specific child record is safe to remove with its parent.

Do not cascade-delete invoices, payments, time entries, balance snapshots or audit history.

## 16.7 Dates and times

Use:

- `DATETIME(6)` for UTC timestamps.
- `DATE` for date-only concepts.

Timestamp examples:

```text
start_utc
completed_utc
date_created_utc
```

Date-only examples:

```text
invoice_date
due_date
maturity_date
```

The application uses a central `IClock` service.

Set database sessions to UTC where practical and never rely on the server's display timezone for business logic.

## 16.8 Money

Use:

```text
DECIMAL(18,2)
```

for stored totals and balances.

Use:

```text
DECIMAL(18,4)
```

for unit rates and quantities where extra precision is needed.

Use:

```text
DECIMAL(7,4)
```

for percentage rates.

## 16.9 Boolean values

Use:

```text
TINYINT(1)
```

## 16.10 Workflow codes

Prefer `VARCHAR` code columns and C# constants/enums rather than MariaDB `ENUM`, because changing enum values requires more intrusive schema alterations.

Code values are lowercase `snake_case`.

## 16.11 Audit columns

Core editable tables should include where appropriate:

```text
date_created_utc
created_by_user_id
date_updated_utc
updated_by_user_id
version_no
date_archived_utc
```

## 16.12 Optimistic concurrency

Updates use `version_no`:

```sql
UPDATE customers
SET
    company_name = @CompanyName,
    date_updated_utc = @DateUpdatedUtc,
    updated_by_user_id = @UpdatedByUserId,
    version_no = version_no + 1
WHERE record_id = @RecordId
  AND version_no = @ExpectedVersionNo;
```

Zero affected rows means the record was changed or removed since it was loaded.

## 16.13 SQL standards

- Parameterise every value.
- Avoid `SELECT *` in application queries.
- List columns explicitly.
- Use transactions for multi-record business operations.
- Use short-lived connections.
- Set command timeouts.
- Pass cancellation tokens.
- Do not share one connection between concurrent operations.
- Keep reporting SQL separate from write repositories.
- Format SQL consistently.
- Review query plans for slow queries.

---

## 17. Proposed database schema

This is the logical baseline. Exact DDL will be created through FluentMigrator.

## 17.1 Security and system tables

### `users`

```text
record_id
username
username_normalised
display_name
password_hash
role_code
is_active
failed_login_count
locked_until_utc
password_changed_utc
last_login_utc
date_created_utc
date_updated_utc
version_no
```

Constraints:

- Unique `username_normalised`.
- Required password hash.

### `password_recovery_codes`

```text
record_id
user_id
recovery_code_hash
created_utc
used_utc
expires_utc
```

Store hashes only.

### `application_settings`

```text
record_id
setting_key
setting_value
value_type_code
is_sensitive
date_updated_utc
updated_by_user_id
```

Constraint:

- Unique `setting_key`.

### `schema_information`

FluentMigrator maintains its own version table. An additional application schema information table may store:

```text
application_version
minimum_supported_application_version
last_verified_utc
```

### `audit_records`

```text
record_id
user_id
entity_type_code
entity_record_id
action_code
action_reason
occurred_utc
old_values_json
new_values_json
correlation_id
```

Indexes:

- `(entity_type_code, entity_record_id, occurred_utc)`.
- `(user_id, occurred_utc)`.
- `correlation_id`.

## 17.2 Customer tables

### `customers`

```text
record_id
company_name
default_hourly_rate
default_payment_terms_days
default_vat_treatment_code
invoice_delivery_code
notes
is_active
date_created_utc
created_by_user_id
date_updated_utc
updated_by_user_id
version_no
date_archived_utc
```

Indexes:

- `company_name`.
- `(is_active, date_archived_utc)`.

### `customer_contacts`

```text
record_id
customer_id
contact_name
job_title
email_address
phone_number
mobile_number
is_primary
notes
date_created_utc
date_updated_utc
version_no
date_archived_utc
```

Indexes:

- `customer_id`.
- `email_address`.

### `customer_addresses`

```text
record_id
customer_id
address_type_code
recipient_name
company_name
address_line_1
address_line_2
town_city
county
postcode
country_code
is_default
date_created_utc
date_updated_utc
version_no
date_archived_utc
```

Indexes:

- `(customer_id, address_type_code)`.
- `postcode`.

## 17.3 Job and work tables

### `jobs`

```text
record_id
customer_id
job_number
job_title
job_description
status_code
priority_code
charging_type_code
estimated_hours
agreed_hourly_rate
fixed_price
start_date
due_date
completed_utc
notes
date_created_utc
created_by_user_id
date_updated_utc
updated_by_user_id
version_no
date_archived_utc
```

Constraints:

- Unique `job_number`.
- Due date not before start date.

Indexes:

- `(customer_id, status_code)`.
- `(status_code, due_date)`.
- `(priority_code, due_date)`.
- `date_archived_utc`.

### `active_timers`

```text
record_id
user_id
job_id
start_utc
work_description
is_billable
date_created_utc
version_no
```

Constraints:

- Unique `user_id`.

### `time_entries`

```text
record_id
user_id
job_id
start_utc
end_utc
raw_duration_seconds
rounded_duration_minutes
entry_method_code
is_billable
work_description
rounding_rule_code
date_created_utc
created_by_user_id
date_updated_utc
updated_by_user_id
version_no
```

Constraints:

- End after start.
- Positive duration.

Indexes:

- `(job_id, start_utc)`.
- `(user_id, start_utc)`.
- `(is_billable, start_utc)`.
- `start_utc`.

### `tasks`

```text
record_id
job_id
task_title
task_notes
status_code
priority_code
due_date
completed_utc
recurrence_definition_id
date_created_utc
created_by_user_id
date_updated_utc
updated_by_user_id
version_no
date_archived_utc
```

Indexes:

- `job_id`.
- `(status_code, due_date)`.
- `(priority_code, due_date)`.

## 17.4 Financial-account tables

### `financial_account_types`

```text
record_id
account_type_code
display_name
classification_code
is_tax_wrapper
is_active
sort_order
```

Constraints:

- Unique `account_type_code`.

### `financial_accounts`

```text
record_id
user_id
account_type_id
account_scope_code
provider_name
account_name
account_reference_last_four
currency_code
account_status_code
current_balance
available_balance
credit_limit
interest_rate
interest_rate_type_code
introductory_rate_end_date
fixed_rate_end_date
maturity_date
opened_date
closed_date
tax_wrapper_code
provider_reference
notes
last_balance_updated_utc
is_hidden
date_created_utc
date_updated_utc
version_no
date_archived_utc
```

Indexes:

- `(user_id, account_scope_code)`.
- `(account_type_id, account_status_code)`.
- `maturity_date`.
- `introductory_rate_end_date`.

### `financial_account_balance_snapshots`

```text
record_id
financial_account_id
balance_at_utc
balance_amount
available_amount
snapshot_source_code
notes
date_created_utc
created_by_user_id
```

Indexes:

- `(financial_account_id, balance_at_utc)`.
- `balance_at_utc`.

### `financial_account_applications`

```text
record_id
user_id
account_type_id
opened_account_id
provider_name
product_name
application_status_code
considered_date
application_date
decision_date
expected_open_date
next_action_date
application_reference
advertised_interest_rate
advertised_bonus_amount
introductory_end_date
application_channel_code
notes
date_created_utc
date_updated_utc
version_no
date_archived_utc
```

Indexes:

- `(application_status_code, next_action_date)`.
- `provider_name`.
- `application_date`.

### `financial_account_contributions`

```text
record_id
financial_account_id
contribution_date
tax_year_start
contribution_type_code
amount
notes
date_created_utc
created_by_user_id
```

Indexes:

- `(financial_account_id, contribution_date)`.
- `(tax_year_start, contribution_date)`.

## 17.5 Invoice tables

### `invoice_number_sequences`

```text
record_id
sequence_code
number_prefix
sequence_year
next_number
version_no
```

Constraint:

- Unique `(sequence_code, sequence_year)`.

### `invoices`

```text
record_id
invoice_number
invoice_type_code
credit_for_invoice_id
customer_id
status_code
invoice_date
due_date
finalised_utc
sent_utc
paid_utc
bill_to_name
bill_to_company
bill_to_address_line_1
bill_to_address_line_2
bill_to_town_city
bill_to_county
bill_to_postcode
bill_to_country_code
bill_to_email_address
currency_code
prices_include_vat
default_vat_rate
net_total
vat_total
gross_total
amount_paid
outstanding_amount
customer_notes
internal_notes
payment_instructions
pdf_relative_path
pdf_sha256_hash
pdf_generated_utc
date_created_utc
created_by_user_id
date_updated_utc
updated_by_user_id
version_no
```

Constraints:

- Unique nullable `invoice_number`.
- Due date not before invoice date.
- Finalised invoices require invoice number.

Indexes:

- `(customer_id, invoice_date)`.
- `(status_code, due_date)`.
- `invoice_date`.
- `credit_for_invoice_id`.

### `invoice_lines`

```text
record_id
invoice_id
line_number
line_type_code
line_description
quantity
unit_rate
discount_type_code
discount_value
discount_amount
vat_rate
net_amount
vat_amount
gross_amount
source_job_id
credit_for_invoice_line_id
date_created_utc
```

Constraint:

- Unique `(invoice_id, line_number)`.

### `invoice_time_entries`

```text
record_id
invoice_line_id
time_entry_id
billed_seconds
billed_minutes
billed_rate
billed_amount
date_created_utc
```

Critical constraint:

- Unique `time_entry_id`.

### `invoice_payments`

```text
record_id
invoice_id
received_into_account_id
payment_date
amount
payment_method_code
payment_reference
notes
is_reversed
reversed_utc
reversal_reason
date_created_utc
created_by_user_id
version_no
```

Indexes:

- `(invoice_id, payment_date)`.
- `(received_into_account_id, payment_date)`.

## 17.6 Expense tables

### `expense_categories`

```text
record_id
category_name
is_active
sort_order
```

Constraint:

- Unique `category_name`.

### `expenses`

```text
record_id
expense_date
supplier_name
expense_category_id
paid_from_account_id
expense_description
net_amount
vat_amount
gross_amount
payment_method_code
payment_reference
is_tax_deductible_estimate
notes
date_created_utc
created_by_user_id
date_updated_utc
updated_by_user_id
version_no
date_archived_utc
```

Indexes:

- `expense_date`.
- `(expense_category_id, expense_date)`.
- `(paid_from_account_id, expense_date)`.

## 17.7 File tables

Use explicit link tables to retain referential integrity.

### `attachments`

```text
record_id
original_file_name
stored_file_name
relative_file_path
content_type
file_size_bytes
sha256_hash
attachment_description
date_created_utc
created_by_user_id
date_archived_utc
```

### Link tables

```text
customer_attachments
job_attachments
expense_attachments
financial_account_attachments
financial_account_application_attachments
```

Each link table contains:

```text
record_id
<entity>_id
attachment_id
date_created_utc
```

Use unique constraints to prevent the same attachment being linked twice to the same entity.

## 17.8 Optional later tables

Do not add until needed:

- `task_recurrence_definitions`.
- `invoice_recurrence_definitions`.
- `quotes`.
- `quote_lines`.
- `personal_transactions`.
- `financial_transfers`.
- `budgets`.
- `savings_goals`.
- `investment_holdings`.
- `investment_valuations`.
- `notification_rules`.

---

## 18. Initial migration order

Recommended migrations:

```text
0001_create_users_and_security
0002_create_application_settings_and_audit
0003_create_customers_contacts_and_addresses
0004_create_jobs_tasks_and_time_tracking
0005_create_financial_account_types_and_accounts
0006_create_account_snapshots_applications_and_contributions
0007_create_invoice_sequences_invoices_and_lines
0008_create_invoice_time_links_and_payments
0009_create_expense_categories_and_expenses
0010_create_attachments_and_link_tables
0011_seed_core_lookup_data
0012_create_required_indexes_and_constraints
0013_seed_initial_application_settings
```

Rules:

- Every migration has an `Up` and safe `Down` where practical.
- Destructive `Down` migrations are not used against production data without explicit approval.
- Back up before applying migrations to an existing production database.
- Never modify a migration already released to a real database.
- Integration tests apply migrations to an empty database.
- Upgrade tests apply migrations to a database created by the previous application version.

---

## 19. Application architecture

## 19.1 Solution structure

```text
PersonalBusinessManager.sln

src/
├── PersonalBusinessManager.WinForms/
├── PersonalBusinessManager.Core/
├── PersonalBusinessManager.Infrastructure/
└── PersonalBusinessManager.Reporting/

tests/
├── PersonalBusinessManager.Core.Tests/
└── PersonalBusinessManager.IntegrationTests/
```

## 19.2 WinForms project

```text
Forms/
Pages/
Dialogs/
Controls/
Navigation/
ViewModels/
Formatting/
Theming/
Validation/
Program.cs
```

Responsibilities:

- Present data.
- Gather input.
- Navigate.
- Invoke services.
- Display validation.
- Apply theme.
- Display loading/error states.

Prohibited:

- SQL.
- Password hashing.
- Invoice calculations.
- Direct backup commands.
- Direct PDF layout code.
- Direct access to database connection strings.

## 19.3 Core project

```text
Domain/
├── Customers/
├── Jobs/
├── TimeTracking/
├── Tasks/
├── Invoicing/
├── BusinessFinance/
├── PersonalFinance/
├── Security/
└── Common/

Application/
├── Contracts/
├── Services/
├── Commands/
├── Queries/
├── Dtos/
├── Filters/
└── Validation/
```

Responsibilities:

- Business rules.
- Workflow transitions.
- Calculation logic.
- Service contracts.
- DTOs.
- Filter models.
- Validation models.

Core must not reference:

- WinForms.
- Dapper.
- MySqlConnector.
- MariaDB-specific classes.
- QuestPDF.

## 19.4 Infrastructure project

```text
Database/
├── MariaDbConnectionFactory.cs
├── Repositories/
├── Queries/
├── Transactions/
├── Migrations/
└── Sql/

Security/
Backups/
Files/
Logging/
Settings/
Clock/
```

Responsibilities:

- Connections.
- Dapper mapping.
- SQL repositories.
- Reporting queries.
- Transactions.
- Migrations.
- Password persistence.
- Credential protection.
- Backups.
- File storage.
- Logging.

## 19.5 Reporting project

```text
Invoices/
BusinessReports/
PersonalFinanceReports/
Templates/
Formatting/
```

Responsibilities:

- QuestPDF layouts.
- Invoice document models.
- Report document models.
- PDF generation.
- File hashing.

## 19.6 Dependency direction

```text
WinForms ───────→ Core
Infrastructure ─→ Core
Reporting ──────→ Core
```

Composition occurs in WinForms startup.

## 19.7 Service contracts

```csharp
ICustomerService
IJobService
ITimeTrackingService
ITaskService
IInvoiceService
IPaymentService
IExpenseService
IBusinessFinanceService
IFinancialAccountService
IAccountApplicationService
IPersonalFinanceReportService
IAuthenticationService
IBackupService
IAttachmentService
IAuditService
```

## 19.8 Repository contracts

Use focused repositories:

```csharp
ICustomerRepository
IJobRepository
ITimeEntryRepository
ITaskRepository
IInvoiceRepository
IPaymentRepository
IExpenseRepository
IFinancialAccountRepository
IAccountApplicationRepository
```

Avoid a generic `IRepository<TEntity>` abstraction that hides the real queries and transaction requirements.

## 19.9 Query classes

Read-heavy lists and reports use dedicated query classes:

```text
CustomerListQuery
JobListQuery
TimeEntryListQuery
TaskListQuery
InvoiceListQuery
ExpenseListQuery
FinancialAccountListQuery
OutstandingInvoiceReportQuery
NetWorthHistoryQuery
```

## 19.10 Connection factory

The connection factory:

- Reads a protected connection string.
- Returns a new unopened or opened connection per operation, according to the chosen convention.
- Does not hold one global connection.
- Supports cancellation.
- Applies required session settings such as UTC.

## 19.11 Transaction orchestration

Transactions belong in application services or an explicit unit-of-work/transaction runner when an operation spans repositories.

Do not begin independent transactions in several repositories for one business operation.

## 19.12 Forms and async code

- Event handlers may be `async void` only where required by WinForms.
- Service methods return `Task`/`Task<T>`.
- Long operations accept `CancellationToken`.
- Disable duplicate action buttons during saves.
- Marshal UI updates safely.
- Do not use `Task.Run` merely to wrap async database methods.

---

## 20. Security and login

## 20.1 Security objectives

The local login provides:

- Casual access control.
- User identity for audit records.
- Automatic application locking.

It does not protect against a Windows administrator reading an unencrypted disk or database backup.

## 20.2 Application passwords

Never store plain-text or reversibly encrypted passwords.

Use a reviewed password hasher such as ASP.NET Core Identity's `PasswordHasher<TUser>` or an approved Argon2id implementation.

Store only the resulting hash string.

## 20.3 Recovery

For the local administrator:

1. Generate a long recovery code.
2. Show it once.
3. Store only its hash.
4. Allow it to set a new password.
5. Mark it used.
6. Generate a replacement.

Do not use security questions.

## 20.4 Login throttling

- Track failed attempts.
- Apply a short temporary lock after repeated failures.
- Do not reveal whether the username or password was incorrect.
- Log security events without storing passwords.

## 20.5 Session handling

Store in memory:

- User ID.
- Display name.
- Role.
- Login time.
- Last activity.

Behaviour:

- Lock after configurable inactivity.
- Require password to unlock.
- Clear session on exit.
- Require reauthentication for restore and security changes.
- Do not implement persistent “remember me” initially.

## 20.6 Roles

Store `role_code`, but initially implement only:

```text
administrator
```

Add granular permissions only when more than one real user requires them.

## 20.7 MariaDB account

Create a dedicated database account such as:

```text
personal_business_app@localhost
```

Rules:

- Do not use MariaDB root.
- Grant only required privileges on the application database.
- Do not grant global privileges.
- Restrict host to localhost.
- Bind the local MariaDB service to `127.0.0.1` while remote access is not required.

A separate migration/admin account may have schema-alter privileges. The normal runtime account should not require unrestricted administration privileges once deployment is mature.

## 20.8 Credential storage

Protect the runtime database credential using:

- Windows Credential Manager, or
- Windows DPAPI with current-user scope.

Do not:

- Commit it to Git.
- Store it unencrypted in a normal configuration file.
- Print it in logs.
- Show it in error dialogs.

## 20.9 Disk and backup protection

- Enable BitLocker or Windows device encryption.
- Restrict application data directories with Windows permissions.
- Encrypt off-device backup archives.
- Store backup encryption credentials separately from the backup.

## 20.10 Sensitive-data rules

Do not log or store unnecessarily:

- Passwords.
- Password hashes.
- Recovery codes.
- Full connection strings.
- Full bank-account numbers.
- Full card numbers.
- Card security codes.
- Banking PINs.
- Online-banking credentials.
- Authentication tokens.
- Identity-document contents.

Use last-four references where sufficient.

## 20.11 SQL injection protection

Every repository value is parameterised.

Incorrect:

```csharp
var sql = $"SELECT * FROM customers WHERE company_name = '{name}'";
```

Correct:

```csharp
const string sql = """
    SELECT record_id AS RecordId, company_name AS CompanyName
    FROM customers
    WHERE company_name = @CompanyName;
    """;
```

## 20.12 Remote security

Never expose MariaDB port `3306` directly to the public internet.

Future architecture:

```text
WinForms
    ↓ HTTPS
ASP.NET Core API
    ↓ private database connection
Hosted MariaDB
```

Optionally place the API behind a VPN for personal access.

---

## 21. Backup, restore and disaster recovery

## 21.1 Backup contents

A full backup contains:

```text
database.sql
attachments/
generated_invoices/
generated_reports/
backup_manifest.json
checksums.txt
```

## 21.2 Database dump

Use `mariadb-dump` with a consistent InnoDB strategy such as:

```text
--single-transaction
--quick
--routines
--triggers
--events
--hex-blob
--default-character-set=utf8mb4
```

Do not perform schema-changing migrations during a `--single-transaction` dump.

Do not put the database password visibly in process arguments.

## 21.3 Backup workflow

1. Verify MariaDB connectivity.
2. Create a temporary backup directory.
3. Run `mariadb-dump`.
4. Check exit code.
5. Validate the dump is present and non-empty.
6. Copy attachments and generated files.
7. Create SHA-256 checksums.
8. Write manifest containing application version, schema version, MariaDB version and timestamp.
9. Compress the backup.
10. Encrypt when stored off-device.
11. Atomically move to final path.
12. Record result in the application log/audit system.
13. Apply retention rules.

## 21.4 Schedule

MVP:

- Automatic backup on first application launch each day.
- Manual Back Up Now.
- Backup before applying migrations.
- Backup before restore.
- Seven daily copies.
- Four weekly copies.
- Optional monthly copies.

## 21.5 Restore workflow

1. Reauthenticate the administrator.
2. Explain that data will be replaced.
3. Back up the current state.
4. Validate archive and checksums.
5. Ensure no active write operation is running.
6. Restore database using the compatible MariaDB client.
7. Restore attachments and generated documents.
8. Apply pending migrations.
9. Run validation checks.
10. Restart/reload the application.
11. Record restore event.

## 21.6 Restore testing

At least periodically:

- Restore into a separate test database.
- Confirm migrations run.
- Confirm customers, jobs, invoices, payments, expenses and accounts load.
- Compare row counts.
- Compare invoice totals.
- Compare current account balances.
- Confirm attachments exist and hashes match.

A backup is not considered reliable until restore testing succeeds.

## 21.7 Crash recovery

At startup:

- Detect previous unclean shutdown.
- Restore active timer display.
- Remove abandoned temporary PDF files.
- Report failed backups.
- Verify database connectivity.
- Run lightweight consistency checks.
- Never automatically rewrite financial records without a logged repair procedure.

---

## 22. Search, filtering and large-list performance

## 22.1 General rule

Do not load unlimited records into WinForms controls.

Filtering, sorting and paging happen in MariaDB before materialisation.

## 22.2 Filter models

Create filter DTOs for:

- Customers.
- Jobs.
- Tasks.
- Time entries.
- Invoices.
- Payments.
- Expenses.
- Financial accounts.
- Account applications.
- Audit records.

Example:

```csharp
public sealed record CustomerFilter(
    string? SearchText,
    bool IncludeArchived,
    int PageSize,
    long? AfterRecordId,
    CustomerSort Sort);
```

## 22.3 Search fields

Customers:

- Company.
- Contact.
- Email.
- Phone.
- Postcode.

Jobs:

- Job number.
- Title.
- Description.
- Customer.

Tasks:

- Title.
- Notes.
- Job.

Time:

- Description.
- Job.
- Customer.

Invoices:

- Invoice number.
- Customer.
- Notes/reference.

Accounts:

- Provider.
- Account name.
- Type.
- Last four digits.

Applications:

- Provider.
- Product.
- Reference.

## 22.4 Debouncing

Debounce free-text search by approximately 250–400 ms and cancel obsolete requests.

## 22.5 Paging

Initial page sizes:

- Customers: 100.
- Jobs: 100.
- Tasks: 100.
- Time: 50.
- Invoices: 50.
- Expenses: 50.
- Account snapshots: 100.
- Audit: 50.

Use keyset pagination for large stable lists where practical:

```sql
WHERE record_id < @LastRecordId
ORDER BY record_id DESC
LIMIT @PageSize;
```

Use offset paging when direct page-number navigation is genuinely required and volumes remain manageable.

## 22.6 Virtual mode

Use `DataGridView.VirtualMode` only when:

- Thousands of rows must be browsed continuously.
- Paging gives a poor workflow.
- A tested row cache exists.

Ordinary paged binding is preferred for the MVP.

## 22.7 Background loading

Use asynchronous database APIs and cancellation tokens.

Display:

- Loading state.
- Retry state.
- Empty state.
- Error state.

Do not freeze the UI during:

- Report queries.
- PDF generation.
- Backup compression.
- File hashing.
- Large exports.

## 22.8 List projections

Return lightweight list DTOs, not full object graphs.

Examples:

```text
CustomerListItem
JobListItem
TimeEntryListItem
InvoiceListItem
FinancialAccountListItem
```

## 22.9 Initial indexes

At minimum:

```text
users(username_normalised UNIQUE)
customers(company_name)
customers(is_active, date_archived_utc)
customer_contacts(customer_id)
customer_contacts(email_address)
jobs(job_number UNIQUE)
jobs(customer_id, status_code)
jobs(status_code, due_date)
active_timers(user_id UNIQUE)
time_entries(job_id, start_utc)
time_entries(user_id, start_utc)
time_entries(is_billable, start_utc)
tasks(status_code, due_date)
tasks(job_id)
invoices(invoice_number UNIQUE)
invoices(customer_id, invoice_date)
invoices(status_code, due_date)
invoice_time_entries(time_entry_id UNIQUE)
invoice_payments(invoice_id, payment_date)
expenses(expense_date)
expenses(expense_category_id, expense_date)
financial_accounts(user_id, account_scope_code)
financial_accounts(account_type_id, account_status_code)
financial_accounts(maturity_date)
financial_account_balance_snapshots(financial_account_id, balance_at_utc)
financial_account_applications(application_status_code, next_action_date)
financial_account_contributions(financial_account_id, contribution_date)
audit_records(entity_type_code, entity_record_id, occurred_utc)
```

Indexes must be reviewed against real query plans rather than added blindly.

---

## 23. Reporting requirements

## 23.1 Business dashboard

- Active timer.
- Time today.
- Tasks overdue.
- Tasks due today.
- Draft invoices.
- Outstanding total.
- Overdue total.
- Current-month invoiced revenue.
- Current-month received revenue.
- Current-month expenses.
- Backup health.

## 23.2 Business reports

- Invoiced revenue by month.
- Received income by month.
- Outstanding invoice ageing.
- Expenses by month.
- Expenses by category.
- Estimated invoiced profit.
- Estimated cash profit.
- Customer revenue.
- Job revenue.
- Job hours.
- Billable versus non-billable time.
- VAT estimate.
- Tax-reserve estimate.

## 23.3 Personal dashboard

- Total assets.
- Total liabilities.
- Estimated net worth.
- Current-account total.
- Savings total.
- ISA total.
- Accounts not updated recently.
- Applications requiring action.
- Upcoming maturity dates.

## 23.4 Personal reports

- Balance by account.
- Assets by type.
- Liabilities by type.
- Net-worth history.
- Monthly balance changes.
- Savings and ISA totals.
- Contributions by tax year.
- Applications by status.
- Upcoming rate expiries.

## 23.5 Export

- CSV for tabular data.
- PDF for formatted summaries.
- Include report title, date range, generated time and scope.
- Never silently include personal data in a business report or vice versa.

---

## 24. Validation and reliability rules

## 24.1 Three validation layers

### UI validation

- Required fields.
- Formatting.
- Immediate feedback.
- Friendly messages.

### Application-service validation

- Business workflows.
- Status transitions.
- Cross-record checks.
- Financial recalculation.
- Permission checks.

### Database validation

- Primary keys.
- Foreign keys.
- Unique constraints.
- Check constraints.
- Non-null columns.
- Transactions.

## 24.2 Duplicate prevention

Protect against:

- Repeated Save clicks.
- Duplicate invoice finalisation.
- Duplicate invoice numbers.
- Duplicate active timers.
- Duplicate time invoicing.
- Duplicate payment submission.
- Duplicate balance updates from retried requests.

Where an operation may be retried, consider a command/correlation ID or idempotency key.

## 24.3 Error handling

Use:

- Global WinForms exception handlers.
- Domain/application exceptions.
- Concurrency exceptions.
- User-friendly messages.
- Correlation IDs.
- Detailed rolling logs.

Do not show raw stack traces to the normal user.

## 24.4 Audit policy

Audit at minimum:

- Time corrections.
- Invoice finalisation.
- Invoice status changes.
- Credit notes.
- Payment creation.
- Payment reversal.
- Expense changes.
- Account balance changes.
- Account application changes.
- Security changes.
- Backup restore.

Audit records are append-only in the normal application.

---

## 25. Testing strategy

## 25.1 Unit tests

Test pure business logic without MariaDB:

- Invoice line calculations.
- VAT calculations.
- Discount calculations.
- Invoice totals.
- Credit limits.
- Payment status calculation.
- Time rounding.
- Duration calculation.
- Overdue calculation.
- Net-worth calculation.
- Profit estimates.
- Tax-reserve estimate.
- Status transitions.

## 25.2 Integration tests

Use MariaDB for:

- Migrations.
- Foreign keys.
- Unique constraints.
- Transactions.
- Invoice number locking.
- Active-timer uniqueness.
- Duplicate time-invoice protection.
- Optimistic concurrency.
- Repository queries.
- Paging.
- Backup/restore scripts where practical.

Do not use an in-memory substitute for tests intended to prove MariaDB behaviour.

## 25.3 UI/manual tests

- Login and lock.
- Navigation.
- Dark theme at common DPI settings.
- Keyboard navigation.
- Customer creation.
- Job creation.
- Timer restart recovery.
- Manual time correction.
- Invoice creation.
- PDF generation.
- Payment entry.
- Expense entry.
- Balance update.
- Application-to-account conversion.
- Backup and restore.

## 25.4 Critical regression tests

Every release should cover:

- No duplicate invoice number.
- No duplicate active timer.
- No duplicate time invoicing.
- Finalised invoice unchanged by customer edits.
- Payment reversal restores correct balance.
- Net worth calculates assets minus liabilities.
- Personal data excluded from business reports.
- Migration preserves existing data.

## 25.5 Test data

Maintain repeatable development seed data containing:

- Several customers.
- Hourly and fixed-price jobs.
- Billable and non-billable time.
- Draft and finalised invoices.
- Partial payment.
- Expense categories.
- Personal asset accounts.
- A liability account.
- Several balance snapshots.
- Account applications in different statuses.

Never use real sensitive personal data in automated tests.

---

## 26. Development roadmap

## Phase 1 — Requirements, wireframes and schema

### Features

- Freeze MVP.
- Confirm statuses and codes.
- Finalise schema.
- Finalise naming rules.
- Create screen wireframes.
- Define dark theme.
- Define financial calculation examples.

### Dependencies

- None.

### Risks

- Scope expansion.
- Mixing business and personal finance.
- Ambiguous invoice rules.

### Completion criteria

- Plan approved.
- Schema reviewed.
- Navigation approved.
- Example calculations agreed.
- Migration order agreed.

## Phase 2 — Solution shell and MariaDB foundation

### Features

- Solution projects.
- Dependency injection.
- Logging.
- MariaDB connection factory.
- Dedicated DB user.
- Dapper.
- FluentMigrator.
- Initial migrations.
- Main dark shell.
- Navigation.
- Reusable themed controls.

### Dependencies

- Phase 1.

### Risks

- Root credentials.
- SQL leaking into forms.
- Inconsistent theme.

### Completion criteria

- Application starts.
- MariaDB connection works.
- Migrations apply.
- Navigation works.
- Sample repository read/write works.
- No SQL in WinForms project.

## Phase 3 — Authentication, settings and audit foundation

### Features

- User setup.
- Password hashing.
- Login.
- Recovery code.
- Session context.
- Inactivity lock.
- Settings service.
- Audit service.

### Dependencies

- Phase 2.

### Risks

- Plain-text credentials.
- Weak recovery.
- Sensitive logging.

### Completion criteria

- Login works.
- Password is hashed.
- Recovery rotates after use.
- Audit records identify user.
- Security logs exclude sensitive values.

## Phase 4 — Customers and jobs

### Features

- Customer list/details.
- Contacts.
- Addresses.
- Jobs.
- Rates.
- Statuses.
- Archive behaviour.
- Attachments foundation.

### Dependencies

- Phase 3.

### Risks

- Oversized forms.
- Hard deletion.
- Broken navigation.

### Completion criteria

- CRUD and archive workflows work.
- Search and paging work.
- Related records open correctly.
- Attachments are managed.

## Phase 5 — Time tracking

### Features

- Active timer.
- Persistent timer strip.
- Start/stop/switch.
- Manual entries.
- Rounding.
- Forgotten timer.
- Corrections and audit.

### Dependencies

- Jobs.
- Authentication.
- Audit.

### Risks

- UTC errors.
- Duplicate timers.
- Lost entries.

### Completion criteria

- Restart recovery works.
- Unique timer enforced.
- Stop transaction is atomic.
- Corrections audited.

## Phase 6 — Tasks

### Features

- General tasks.
- Job tasks.
- Statuses.
- Priorities.
- Due-date views.
- Dashboard cards.

### Dependencies

- Jobs.
- Common list controls.

### Risks

- Premature recurrence.

### Completion criteria

- Create, edit, complete, reopen and filter work.
- Dashboard counts reconcile.

## Phase 7 — Financial accounts foundation

### Features

- Account types.
- Personal/business scope.
- Account list/details.
- Balance snapshots.
- Account applications.
- Contributions.

### Dependencies

- Authentication.
- Audit.
- Attachments.

### Risks

- Incorrect sign handling.
- Sensitive data storage.
- Mixed scopes.

### Completion criteria

- Balance updates create snapshots.
- Net-worth calculation is correct.
- Application converts to account.
- No banking credentials stored.

## Phase 8 — Invoicing and PDFs

### Features

- Draft invoice.
- Time selection.
- Manual lines.
- Discounts.
- VAT.
- Billing snapshot.
- Number sequence.
- Finalisation.
- QuestPDF.
- Credit notes.

### Dependencies

- Customers.
- Jobs.
- Time.
- Settings.
- Audit.

### Risks

- Duplicate invoicing.
- Incorrect totals.
- PDF mismatch.

### Completion criteria

- Finalisation atomic.
- Unique number enforced.
- Time cannot be invoiced twice.
- PDF matches database.
- Finalised invoice read-only.

## Phase 9 — Payments, expenses and business accounts

### Features

- Payments.
- Part payments.
- Reversals.
- Expenses.
- Categories.
- Receipts.
- Business account links.

### Dependencies

- Invoices.
- Accounts.
- Attachments.

### Risks

- Incorrect balances.
- Personal/business mixing.

### Completion criteria

- Invoice outstanding amount reconciles.
- Payment reversal works.
- Expenses report correctly.
- Business-account filters enforced.

## Phase 10 — Dashboards and reporting

### Features

- Business dashboard.
- Personal dashboard.
- Business reports.
- Personal reports.
- CSV.
- PDF reports.

### Dependencies

- Main modules complete.

### Risks

- Misleading estimates.
- Slow queries.

### Completion criteria

- Reports reconcile to source rows.
- Scope separation is correct.
- Estimate notices visible.
- Exports work.

## Phase 11 — Backup, restore and operational hardening

### Features

- Automatic dump.
- Complete archive.
- Checksums.
- Restore UI.
- Restore validation.
- Retention.
- Crash-recovery checks.

### Dependencies

- Stable schema and file storage.

### Risks

- Missing attachments.
- Unrestorable archive.
- Exposed credentials.

### Completion criteria

- Clean restore succeeds.
- Row totals reconcile.
- Attachments validate.
- Backup failure is visible.

## Phase 12 — Testing, installer and release

### Features

- Unit tests.
- Integration tests.
- Migration tests.
- Installer.
- Upgrade process.
- User guide.

### Dependencies

- MVP complete.

### Risks

- Installer overwrites data.
- MariaDB dependency confusion.

### Completion criteria

- Clean install works.
- Upgrade works.
- App runs without Visual Studio.
- Critical tests pass.
- Backup/restore documented.

## Phase 13 — Later remote-access architecture

### Features

- ASP.NET Core API.
- Hosted MariaDB.
- HTTPS authentication.
- API client service implementations.
- Server backup.
- Optional multi-user support.

### Dependencies

- Stable local service boundaries.

### Risks

- Public database exposure.
- Authentication flaws.
- Migration errors.
- Concurrency conflicts.

### Completion criteria

- Database is private.
- Desktop connects through API only.
- Totals reconcile after migration.
- Server backup/restore tested.

---

## 27. Remote-access migration path

### Current

```text
WinForms
    ↓
Application services
    ↓
Repositories and query classes
    ↓
Local MariaDB
```

### Future

```text
WinForms
    ↓ HTTPS
ASP.NET Core API
    ↓
Server-side application services
    ↓
Hosted MariaDB
```

### Migration principles

- Keep service interfaces stable where practical.
- Add API-backed implementations.
- Do not place hosted database credentials in the desktop application.
- Move authorisation and final business validation to the server.
- Preserve primary keys during migration where safe.
- Transfer attachments to secure server/object storage.
- Compare row counts and financial totals.
- Keep a rollback backup.

### Direct database warning

Do not expose MariaDB to the internet even if a strong password is used. The later desktop client should authenticate to an API or operate through a private VPN-controlled service.

---

## 28. Coding standards for Codex

## 28.1 General

- Enable nullable reference types.
- Prefer clear code over clever abstraction.
- Use small, focused classes.
- Use dependency injection.
- Use `async` database APIs.
- Pass cancellation tokens.
- Avoid static mutable global state.
- Avoid service locator patterns.
- Keep methods focused.
- Use guard clauses.
- Document non-obvious business rules.

## 28.2 WinForms

- No SQL in forms/pages/controls.
- No direct password hashing.
- No financial calculation duplication.
- Disable Save while a save is in progress.
- Prevent duplicate event subscriptions.
- Dispose forms, controls and cancellation sources correctly.
- Centralise formatting.
- Centralise theme values.
- Use DTOs designed for the screen.

## 28.3 SQL

- Parameterise values.
- Explicit columns.
- Consistent aliases.
- Transactions for workflows.
- Limit rows.
- Sort deterministically.
- Use indexes deliberately.
- Use `FOR UPDATE` only inside a transaction.
- Do not silently swallow database exceptions.

## 28.4 Services

A service method should:

1. Validate request.
2. Authorise user where relevant.
3. Begin transaction where required.
4. Load required current state.
5. Enforce transition/rules.
6. Perform writes.
7. Write audit record.
8. Commit.
9. Return a result DTO.

## 28.5 Logging

Log:

- Operation name.
- Correlation ID.
- Entity type and safe identifier.
- Error category.
- Duration for slow operations.

Do not log:

- Passwords.
- Recovery codes.
- DB credentials.
- Full financial identifiers.
- Entire customer records.

## 28.6 Comments

Comments should explain why, not restate the code.

## 28.7 Commits

Prefer small coherent commits such as:

```text
feat(customers): add paged customer list
feat(time): persist active timers
fix(invoices): prevent duplicate finalisation
migration(accounts): add balance snapshots
```

---

## 29. Definition of done

A feature is not complete until:

- Requirements are satisfied.
- UI follows dark-theme standards.
- SQL is outside WinForms.
- Validation exists at appropriate layers.
- Transactions protect multi-record writes.
- Audit records are added where required.
- Errors are handled.
- Logging excludes sensitive data.
- Unit tests cover business rules.
- Integration tests cover MariaDB-specific behaviour where relevant.
- Migration exists for schema changes.
- Existing data upgrade is considered.
- Manual test steps pass.
- No known critical regression remains.
- Relevant documentation is updated.

---

## 30. Recommended Codex prompt template

Use a prompt similar to this for each implementation slice:

```text
You are updating my C# WinForms Personal Business Manager application.

Treat `personal_business_management_application_final_plan.md` as the source of truth.

Task:
[Describe one focused feature or change.]

Before changing code:
1. Read the relevant plan sections.
2. Inspect the existing solution structure and related files.
3. Identify affected projects, services, repositories, migrations and tests.
4. Preserve the architecture: no SQL in WinForms, parameterised SQL only, business logic in services, MariaDB migrations through FluentMigrator.

Implementation requirements:
- Keep database names lowercase snake_case.
- Keep C# names in normal PascalCase/camelCase.
- Use async APIs and cancellation tokens.
- Use transactions for multi-table operations.
- Preserve optimistic concurrency.
- Add audit records for relevant financial or correction operations.
- Preserve the dark-theme design system.
- Do not store or log sensitive credentials.
- Do not implement unrelated later-scope features.

Testing:
- Add or update unit tests.
- Add MariaDB integration tests for constraints, transactions or queries where relevant.
- Provide manual test steps.

At the end, report:
- Summary.
- Files changed.
- Migration added.
- Tests added/updated.
- Manual test steps.
- Known limitations or follow-up work.
```

---

## 31. Key risks and mitigations

### Risk: scope expansion

**Mitigation:** Keep personal finance limited to accounts, balances, applications, contributions and reports in the MVP.

### Risk: unmaintainable forms

**Mitigation:** Use thin pages, services, repositories, reusable controls and one shell.

### Risk: SQL spread throughout the project

**Mitigation:** Keep SQL only in Infrastructure repositories/query classes and enforce through review.

### Risk: invoice corruption

**Mitigation:** Immutable finalised records, transactions, unique constraints, audits and tests.

### Risk: duplicate invoicing

**Mitigation:** Unique `invoice_time_entries.time_entry_id`.

### Risk: duplicate invoice numbers

**Mitigation:** Sequence row, `FOR UPDATE`, transaction and unique index.

### Risk: lost timers

**Mitigation:** `active_timers` table and atomic stop operation.

### Risk: personal/business data mixing

**Mitigation:** Mandatory account scope and separate report queries.

### Risk: misleading finance/tax figures

**Mitigation:** Estimate labels, transparent formulas and no claim of professional advice.

### Risk: sensitive financial exposure

**Mitigation:** No banking credentials, BitLocker, restricted folders, protected DB credentials and encrypted backups.

### Risk: failed backups

**Mitigation:** Exit-code checking, checksums, manifests and restore tests.

### Risk: dark-theme inconsistency

**Mitigation:** Theme tokens and reusable controls created before feature screens.

### Risk: UI freezing

**Mitigation:** Async I/O, paging, cancellation, background report/PDF/backup operations.

### Risk: future remote-access rewrite

**Mitigation:** Forms depend on service interfaces rather than database implementations.

---

## 32. Final recommended first version

The first production-capable release should contain:

- .NET 10 WinForms application.
- Dark application shell.
- MariaDB LTS database.
- InnoDB and `utf8mb4`.
- Dedicated localhost database account.
- Protected connection credentials.
- MySqlConnector.
- Dapper.
- FluentMigrator.
- Lowercase `snake_case` database objects.
- `record_id` primary keys.
- Secure application login.
- Customers, contacts and addresses.
- Jobs with hourly, fixed and mixed charging.
- Persistent single active timer.
- Manual and audited time records.
- Tasks.
- Draft/finalised invoices.
- Unique invoice numbers.
- Duplicate time-invoice protection.
- Configurable VAT.
- Line-level discounts.
- QuestPDF invoice output.
- Payments, part payments and reversals.
- Credit notes.
- Expenses and receipt attachments.
- Business accounts.
- Personal accounts.
- Account balance history.
- Asset and liability totals.
- Estimated net worth.
- Account applications.
- ISA/savings contribution records.
- Business reports.
- Personal reports.
- CSV exports.
- Automatic backups.
- Tested restore.
- Audit history.
- Database migrations.
- Archive rather than destructive deletion.

Delay until later:

- Quotes.
- Recurring records.
- Email sending.
- Personal transactions.
- Transfers.
- Budgets.
- Bank feeds.
- Live investments.
- Multiple users.
- Light theme.
- Remote access.
- Full accounting.

---

## 33. Decision log

| Decision | Status | Reason |
|---|---:|---|
| C# WinForms desktop client | Approved | Familiar, achievable Windows desktop framework. |
| .NET 10 LTS | Approved | Supported modern baseline for a new application. |
| Local MariaDB | Approved | Matches existing experience and future hosted database direction. |
| MariaDB maintained LTS baseline | Approved | Avoid relying indefinitely on an old XAMPP database version. |
| MySqlConnector | Approved | Modern async ADO.NET connector compatible with MariaDB. |
| Dapper | Approved | Explicit, lightweight SQL mapping. |
| FluentMigrator | Approved | Database migrations without requiring a full ORM. |
| QuestPDF | Approved with licence review | Suitable C# PDF generation; licence must be rechecked when deployment circumstances change. |
| Lowercase snake_case DB naming | Approved | Developer preference and consistency. |
| `record_id` primary key naming | Approved | Developer preference and consistency. |
| Dark theme from first release | Approved | Required product design. |
| Personal-finance account tracking in MVP | Approved | Core user requirement. |
| Full personal transaction tracking | Deferred | Excessive scope for initial release. |
| Direct internet MariaDB access | Rejected | Unacceptable remote-access security model. |
| API-based future access | Approved direction | Preserves secure server-side access and validation. |

---

## 34. Change log

### Version 1.0 — 27 July 2026

- Created the consolidated final development plan.
- Selected local MariaDB rather than SQLite.
- Standardised database naming on lowercase `snake_case` and `record_id`.
- Added personal financial accounts, balance history, applications, contributions and net-worth tracking.
- Added dark-theme architecture.
- Added Codex implementation rules.
- Added migration order, test strategy and definition of done.
- Added explicit business/personal finance separation.

---

## 35. Glossary

**Application service**  
A class that coordinates a business operation such as finalising an invoice or stopping a timer.

**Audit record**  
Append-only evidence of an important change, including who performed it and what changed.

**Balance snapshot**  
The recorded value of a financial account at a particular time.

**Core project**  
The project containing business rules, interfaces, DTOs and validation without UI or database dependencies.

**Credit note**  
A financial document that reverses all or part of a previous invoice.

**Dapper**  
A lightweight .NET object mapper used to execute parameterised SQL and map results.

**DTO**  
Data transfer object used to move purpose-specific data between layers.

**Finalised invoice**  
An invoice whose number, customer snapshot, lines and totals are locked.

**FluentMigrator**  
A versioned database migration framework.

**Infrastructure project**  
The project implementing database, file, backup, logging and security concerns.

**Keyset pagination**  
Paging using the last seen indexed key rather than increasingly large row offsets.

**Management estimate**  
A planning figure that is not a formal accounting or tax result.

**Optimistic concurrency**  
Preventing accidental overwrites by checking that a version value has not changed.

**Repository**  
An Infrastructure component responsible for focused data persistence operations.

**Service interface**  
A contract used by the UI without tying it to local MariaDB or a future API.

**Soft deletion/archiving**  
Hiding an inactive record while preserving its historical relationships.

**UTC**  
The consistent timestamp basis used for stored event times.

---

## 36. Version-sensitive official references

These references are included because software support and licensing may change. Recheck them before major upgrades or production deployment.

- [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)
- [.NET 10 downloads](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [MariaDB 11.8 LTS announcement](https://mariadb.org/11-8-lts-released/)
- [MariaDB releases](https://mariadb.org/mariadb/all-releases/)
- [MySqlConnector documentation](https://mysqlconnector.net/)
- [MySqlConnector best practices](https://mysqlconnector.net/tutorials/best-practices/)
- [Dapper repository](https://github.com/DapperLib/Dapper)
- [FluentMigrator documentation](https://fluentmigrator.github.io/)
- [FluentMigrator MySQL/MariaDB provider](https://fluentmigrator.github.io/providers/mysql.html)
- [QuestPDF](https://www.questpdf.com/)
- [QuestPDF Community Licence](https://www.questpdf.com/license/community.html)
- [MariaDB `mariadb-dump`](https://mariadb.com/docs/server/clients-and-utilities/backup-restore-and-import-clients/mariadb-dump)
- [MariaDB restore guide](https://mariadb.com/docs/server/mariadb-quickstart-guides/mariadb-restore-guide)
- [MariaDB bind address documentation](https://mariadb.com/docs/server/server-management/variables-and-modes/server-system-variables)
- [MariaDB remote connection guidance](https://mariadb.com/docs/server/server-usage/connecting/mariadb-remote-connection-guide-1)

---

# End of baseline plan
