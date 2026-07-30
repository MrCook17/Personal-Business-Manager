# 12 — Business Finance Workspace

> **Project:** Personal Business Manager  
> **Phase:** P1-04 — Low-fidelity wireframes  
> **Design status:** Approved working baseline  
> **Owner:** Charlie Cook  
> **Decision date:** 29 July 2026  
> **Platform:** C# WinForms, desktop-first, dark theme  
> **Default locale:** `en-GB`, GBP  
> **Implementation rule:** Forms and controls contain no SQL; pages call application services.


## Purpose

Define coordinated expense, payment, business-account and management-report screens while keeping the concise sidebar.

**Primary route:** `Business Finance > Expenses or Business Reports`

## Shared visual rules

- Use shared dark-theme tokens and the 4/8/16/24/32 spacing system.
- Prefer Segoe UI or Segoe UI Variable.
- Use `AutoScaleMode.Dpi`; verify at 100%, 125% and 150% scaling.
- Maintain visible keyboard focus and do not communicate status using colour alone.
- Main pages are reusable `UserControl` instances hosted by the main shell.
- Use dialogs only for focused create, edit, confirmation and correction workflows.
- Long operations are asynchronous and must not freeze the UI.


## Route model

The permanent sidebar uses:

```text
Invoices
Expenses
Business Reports
```

The shared workspace uses:

```text
[Overview] [Payments] [Expenses] [Business accounts] [Reports]
```

Opening Expenses selects Expenses. Opening Business Reports selects Overview/Reports.

## Overview

```text
Date range [This month ▾] [Refresh]
[Invoiced £6,400] [Received £5,100] [Expenses £1,340]
[Invoiced profit est. £5,060] [Cash profit est. £3,760]
[Output VAT £1,080] [Input VAT £120] [VAT estimate £960]
[Tax reserve estimate £1,012]

Planning estimates only; not accounting or tax advice.
[Open source rows] [Export PDF] [Export CSV]
```

Charts may be added only with accessible table/source-row equivalents.

## Payments

Paged list:

```text
Date Invoice Customer Amount Method Account Reference Reversed
```

Filters: date, customer, invoice, method, receiving account and reversed state.

Actions: open invoice, reverse with reason, export CSV. Payments are reversed, never edited/deleted.

## Expenses

```text
Expenses                              [Export CSV] [+ Add expense]
Search [supplier, description, reference________]
Date [This month ▾] Category [All ▾] Account [All ▾]
Tax estimate [All ▾] [ ] Include archived

Date Supplier Category Description Net VAT Gross Paid from Receipt
```

Default page size: **50**.

Dialog fields: date, supplier, category, description, net/VAT/gross, business account, payment method, reference, estimated deductibility, notes and receipt.

## Business accounts

Only `account_scope_code = business`.

```text
Account Provider Type Status Current balance Last updated
```

Actions: add account, open details, update balance.

## Reports

Selectors include revenue, received income, ageing, expenses, profit estimates, customer/job revenue, hours, VAT and tax-reserve estimate.

Controls: date range, relevant entity filters, gross/net where appropriate, run, CSV and PDF.

Every output includes title, filters, generated time and `Business` scope.

## States

| State | Presentation |
|---|---|
| Empty list | Relevant add/navigation action. |
| Loading report | Cancellable result-area progress. |
| Error | Section retry and correlation ID. |
| Validation | Expense/payment/reversal messages. |
| Estimate unavailable | Explain missing data/configuration; no misleading zero. |
| Archived expense | Hidden by default and labelled. |
| Reversed payment | Retained with timestamp and reason. |

## Navigation

Invoice/payment → Invoice viewer.  
Customer/job report row → details.  
Expense → view/edit dialog.  
Business account → Account details.  
Report source rows → underlying filtered list.

## Excluded from MVP

No general ledger, payroll, bank reconciliation, Open Banking, automatic banking login, or automatic tax/VAT submission.

## Scope boundaries

- Implement only the approved MVP behaviour shown here.
- Do not add speculative controls or infrastructure for deferred features.
- Later changes must update the final plan and this wireframe first.

## Approval record

This file forms part of the P1-04 working baseline authorised by the owner’s instruction to complete the full wireframe set. Committing it records acceptance unless a later approved decision supersedes it.
