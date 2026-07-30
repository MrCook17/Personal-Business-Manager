# 03 — Dashboard

> **Project:** Personal Business Manager  
> **Phase:** P1-04 — Low-fidelity wireframes  
> **Design status:** Approved working baseline  
> **Owner:** Charlie Cook  
> **Decision date:** 29 July 2026  
> **Platform:** C# WinForms, desktop-first, dark theme  
> **Default locale:** `en-GB`, GBP  
> **Implementation rule:** Forms and controls contain no SQL; pages call application services.


## Purpose

Surface urgent work, business performance and personal-finance summaries with direct navigation to source records.

**Primary route:** `Dashboard`

## Shared visual rules

- Use shared dark-theme tokens and the 4/8/16/24/32 spacing system.
- Prefer Segoe UI or Segoe UI Variable.
- Use `AutoScaleMode.Dpi`; verify at 100%, 125% and 150% scaling.
- Maintain visible keyboard focus and do not communicate status using colour alone.
- Main pages are reusable `UserControl` instances hosted by the main shell.
- Use dialogs only for focused create, edit, confirmation and correction workflows.
- Long operations are asynchronous and must not freeze the UI.


## Normal layout

```text
┌─────────────────────────────────────────────────────────────────────────────────┐
│ Dashboard                                               [New ▾] [Refresh]       │
│ Updated 18:10                                           Backup ✓                │
├─────────────────────────────────────────────────────────────────────────────────┤
│ ACTIVE TIMER                                                                    │
│ Acme / JOB-0042 / Website maintenance / 01:18:43 [Stop] [Open job]             │
├─────────────────────────────────────────────────────────────────────────────────┤
│ BUSINESS OVERVIEW                                                               │
│ [Overdue tasks 3] [Due today 5] [Outstanding £4,820] [Overdue £1,250]          │
│ [Invoiced £6,400] [Received £5,100] [Expenses £1,340]                          │
│                                                                                 │
│ Tasks requiring attention             Outstanding invoices                     │
│ Due   Task       Job Priority          Invoice Customer Due Outstanding          │
│ ...                                     ...                                     │
│ [View all tasks]                       [View outstanding invoices]               │
├─────────────────────────────────────────────────────────────────────────────────┤
│ PERSONAL FINANCE                                                               │
│ [Assets £31,250] [Liabilities £1,100] [Net worth £30,150] [Savings £28,000]    │
│ Accounts needing updates              Applications requiring action            │
│ ...                                   ...                                       │
│ [View accounts]                       [View applications]                       │
└─────────────────────────────────────────────────────────────────────────────────┘
```

## New menu

```text
New customer
New job
Start timer
New task
New invoice
New expense
Update account balance
New account application
```

## Interaction rules

- Summary cards open the matching filtered list.
- Business and personal scopes never mix.
- Estimates are labelled as estimates.
- No custom dashboard layout is included.
- Each section loads independently so one failure does not blank the page.
- Only the visible elapsed timer updates every second.

## States

| State | Presentation |
|---|---|
| Empty business | Stable zero cards plus **Add first customer/task**. |
| Empty personal finance | Stable zero cards plus **Add first account**. |
| Loading | Card/list skeletons; timer remains usable. |
| Partial error | Error and retry inside only the affected section. |
| Full error | Safe page-level retry. |
| Stale data | Show last-updated time; never replace failed values with zero. |

## Detail navigation

- Task row → task edit/details.
- Invoice row → invoice viewer.
- Account row → account details.
- Application row → application details.
- Cards apply the appropriate filter to the destination screen.

## Scope boundaries

- Implement only the approved MVP behaviour shown here.
- Do not add speculative controls or infrastructure for deferred features.
- Later changes must update the final plan and this wireframe first.

## Approval record

This file forms part of the P1-04 working baseline authorised by the owner’s instruction to complete the full wireframe set. Committing it records acceptance unless a later approved decision supersedes it.
