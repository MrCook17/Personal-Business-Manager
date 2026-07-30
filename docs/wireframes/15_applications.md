# 15 — Financial Account Applications

> **Project:** Personal Business Manager  
> **Phase:** P1-04 — Low-fidelity wireframes  
> **Design status:** Approved working baseline  
> **Owner:** Charlie Cook  
> **Decision date:** 29 July 2026  
> **Platform:** C# WinForms, desktop-first, dark theme  
> **Default locale:** `en-GB`, GBP  
> **Implementation rule:** Forms and controls contain no SQL; pages call application services.


## Purpose

Track financial-product applications, next actions and conversion into linked accounts without automating applications or storing provider credentials.

**Primary route:** `Personal Finance > Applications`

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
│ Account applications                    [Export CSV] [+ Add application]         │
├──────────────────────────────────────────────────────────────────────────────────┤
│ [Needs action 2] [In progress 3] [Approved 1] [Completed 8]                    │
│ Search [provider, product, reference____________]                               │
│ Status [Active ▾] Type [All ▾] Next action [Any ▾]                             │
│ [ ] Include archived [Clear]                                                     │
├──────────────────────────────────────────────────────────────────────────────────┤
│ Provider Product       Type      Status         Applied   Next action Rate       │
│ Bank A   Regular Saver Savings   Awaiting info  25/07/26 30/07/26   7.00%       │
│ Selected: [Open] [Edit] [Convert to account] [Complete] [Archive]               │
├──────────────────────────────────────────────────────────────────────────────────┤
│ 1–100 of 14                   [Previous] Page 1 [Next] Rows [100 ▾]              │
└──────────────────────────────────────────────────────────────────────────────────┘
```

## Add/edit dialog

```text
Provider *          [____________________________]
Product name *      [____________________________]
Account type *      [Savings account ▾]
Status              [Considering ▾]
Considered/applied/decision/expected dates [...]
Next action date    [dd/mm/yyyy]
Reference           [____________________________]
Advertised rate     [____ %]
Advertised bonus    [£________]
Promotional end     [dd/mm/yyyy]
Channel             [Online ▾]
Notes               [____________________________]
[Cancel] [Save application]
```

## Workflow

Friendly values map to the approved codes:

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

Only valid next statuses are offered.

## Convert to account

```text
Open account from approved application
Provider/product/type/rate/dates are pre-filled.
Reference last four [____]
Opening balance     [£________]
Opened date *       [dd/mm/yyyy]
[Cancel] [Create and link account]
```

One transaction creates the account, optional initial snapshot, link and audit event, then sets application status to opened.

## States

| State | Presentation |
|---|---|
| Empty | **Add application**. |
| Loading | Summary/list load independently. |
| Error | Retry; retain filters. |
| Validation | Status/date/required-field messages. |
| Needs action | Due/overdue text and icon. |
| Approved | Prominent Convert action. |
| Opened | Linked account; duplicate conversion disabled. |
| Completed/declined/withdrawn | Historical state. |
| Archived | Hidden by default and labelled. |

## Paging and navigation

Default page size: **100**.

Row → details/edit.  
Linked account → Account details.  
Dashboard card → needs-action filter.

## Excluded from MVP

No automatic application, Open Banking, automatic banking login or provider authentication storage.

## Scope boundaries

- Implement only the approved MVP behaviour shown here.
- Do not add speculative controls or infrastructure for deferred features.
- Later changes must update the final plan and this wireframe first.

## Approval record

This file forms part of the P1-04 working baseline authorised by the owner’s instruction to complete the full wireframe set. Committing it records acceptance unless a later approved decision supersedes it.
