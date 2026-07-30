# 13 — Personal Account List

> **Project:** Personal Business Manager  
> **Phase:** P1-04 — Low-fidelity wireframes  
> **Design status:** Approved working baseline  
> **Owner:** Charlie Cook  
> **Decision date:** 29 July 2026  
> **Platform:** C# WinForms, desktop-first, dark theme  
> **Default locale:** `en-GB`, GBP  
> **Implementation rule:** Forms and controls contain no SQL; pages call application services.


## Purpose

Show personal assets, liabilities and net worth with safe account creation and manual balance updates.

**Primary route:** `Personal Finance > Accounts`

## Shared visual rules

- Use shared dark-theme tokens and the 4/8/16/24/32 spacing system.
- Prefer Segoe UI or Segoe UI Variable.
- Use `AutoScaleMode.Dpi`; verify at 100%, 125% and 150% scaling.
- Maintain visible keyboard focus and do not communicate status using colour alone.
- Main pages are reusable `UserControl` instances hosted by the main shell.
- Use dialogs only for focused create, edit, confirmation and correction workflows.
- Long operations are asynchronous and must not freeze the UI.


## Shared list behaviour

- Search is debounced by approximately 250–400 ms.
- A newer search cancels the obsolete request.
- Filtering, sorting and paging occur in MariaDB.
- Grids use deterministic sorting, double buffering and explicit states.
- `Enter` or double-click opens the selected record.
- Archive visibility is an explicit filter.

## Layout

```text
┌──────────────────────────────────────────────────────────────────────────────────┐
│ Personal accounts                         [Export CSV] [+ Add account]           │
├──────────────────────────────────────────────────────────────────────────────────┤
│ [Assets £31,250] [Liabilities £1,100] [Net worth £30,150]                       │
│ [Current £2,250] [Savings £18,000] [ISAs £11,000]                              │
│ Estimates use latest recorded balances.                                         │
├──────────────────────────────────────────────────────────────────────────────────┤
│ Search [provider, name, last four____________]                                  │
│ Type [All ▾] Class [All ▾] Status [Open ▾] Updates [All ▾]                     │
│ [ ] Show hidden [ ] Include archived [Clear]                                    │
├──────────────────────────────────────────────────────────────────────────────────┤
│ Provider Account      Type       Class     Status Balance Updated                │
│ Bank A   Everyday 1234 Current   Asset     Open   £1,250  Today                  │
│ Selected: [Open] [Update balance] [Edit] [Hide]                                 │
├──────────────────────────────────────────────────────────────────────────────────┤
│ 1–100 of 12                   [Previous] Page 1 [Next] Rows [100 ▾]              │
└──────────────────────────────────────────────────────────────────────────────────┘
```

The route is fixed to personal scope. Business accounts never appear here.

## Add account

Fields: provider, account name/type, last four, status, opening/current and available balances, credit limit where relevant, interest rate/type, key dates, tax wrapper and notes.

## Update balance

```text
Account: Everyday 1234
Current recorded balance £1,250.00
Balance at * [29/07/2026 18:10]
New balance * [£________]
Available [£________]
Source [Manual ▾]
Notes [________________]
[Cancel] [Save balance]
```

One transaction creates the snapshot and updates current values.

## Semantics

- Net worth = assets minus liabilities.
- Negative asset balances reduce net worth.
- Negative liability balances represent credit.
- Contributions do not automatically change balances.

## States

| State | Presentation |
|---|---|
| Empty | **Add account**. |
| Loading | Cards and list load independently. |
| Error | Retry; failed values are not shown as zero. |
| Validation | Account-type-specific messages. |
| Stale | Text/icon based on update age. |
| Hidden | Excluded unless requested. |
| Archived | Excluded by default. |
| Scope mismatch | Data-integrity error; never show business data. |

## Paging and navigation

Default page size: **100**.

Account → Account details.  
Summary card → same list with filter.  
Dashboard → matching filtered account view.

## Security

Never request or store banking passwords, PINs, full account/card numbers, security codes or authentication secrets.

## Scope boundaries

- Implement only the approved MVP behaviour shown here.
- Do not add speculative controls or infrastructure for deferred features.
- Later changes must update the final plan and this wireframe first.

## Approval record

This file forms part of the P1-04 working baseline authorised by the owner’s instruction to complete the full wireframe set. Committing it records acceptance unless a later approved decision supersedes it.
