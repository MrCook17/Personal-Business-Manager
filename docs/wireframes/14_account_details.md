# 14 — Financial Account Details

> **Project:** Personal Business Manager  
> **Phase:** P1-04 — Low-fidelity wireframes  
> **Design status:** Approved working baseline  
> **Owner:** Charlie Cook  
> **Decision date:** 29 July 2026  
> **Platform:** C# WinForms, desktop-first, dark theme  
> **Default locale:** `en-GB`, GBP  
> **Implementation rule:** Forms and controls contain no SQL; pages call application services.


## Purpose

Show current account state, balance history, contributions, attachments, notes and audit history without creating a transaction ledger.

**Primary route:** `Personal Finance > Accounts > {Account}`

## Shared visual rules

- Use shared dark-theme tokens and the 4/8/16/24/32 spacing system.
- Prefer Segoe UI or Segoe UI Variable.
- Use `AutoScaleMode.Dpi`; verify at 100%, 125% and 150% scaling.
- Maintain visible keyboard focus and do not communicate status using colour alone.
- Main pages are reusable `UserControl` instances hosted by the main shell.
- Use dialogs only for focused create, edit, confirmation and correction workflows.
- Long operations are asynchronous and must not freeze the UI.


## Header

```text
Accounts / Bank A — Everyday
Bank A — Everyday 1234                        Status: Open
Balance £1,250.00  Available £1,100.00        Updated 29/07/2026 18:10
[Update balance] [Edit] [Close account] [⋯]
```

Overflow: hide, archive, export history.

## Tabs

```text
[Overview] [Balance history] [Contributions] [Attachments] [Notes] [Activity]
```

## Overview

```text
Account information                  Rate and dates
Provider        Bank A               Interest rate 5.00% fixed
Type            Current account      Intro end     —
Classification  Asset               Fixed end     —
Scope           Personal            Maturity      —
Status          Open                Opened/closed ...
Currency        GBP
```

## Balance history

```text
[Update balance] [Export CSV]
Date/time            Balance    Available Source    Notes
29/07/26 18:10       £1,250     £1,100    Manual    ...
```

Default page size: **100**, newest first. Snapshots are immutable in normal UI.

## Contributions

```text
[Add contribution] Tax year [2026/27 ▾]
Date Type Amount Tax year Notes
```

Permanent note: contributions are informational and do not automatically change balance.

## Attachments, notes and activity

- Attachments: terms, confirmations, rate notices and deliberately stored statements.
- Warn against unnecessary identity-document storage.
- Notes use explicit save.
- Activity shows balance, status, application-conversion and archive events.

## Status behaviour

- Close requires a closed date.
- Closing retains all history.
- Hide is separate from status/archive.
- Archive removes from normal lists but retains records.
- Business-scope reuse must be clearly labelled in header and breadcrumb.

## States

| State | Presentation |
|---|---|
| Open | Update/edit/close actions. |
| Dormant/restricted | Persistent status banner. |
| Closed | Historical view; updates disabled unless correction policy allows. |
| Hidden | Notice. |
| Archived | Banner and Restore. |
| No snapshots | **Update balance** and explanation. |
| Loading/error | Tab-local. |
| Validation | Type/date consistency. |
| Concurrency | Reload and compare. |

## Navigation and exclusions

Originating application → Application details.  
Activity → Audit detail.

No personal transactions, transfer ledger, live investment prices or holdings in MVP.

## Scope boundaries

- Implement only the approved MVP behaviour shown here.
- Do not add speculative controls or infrastructure for deferred features.
- Later changes must update the final plan and this wireframe first.

## Approval record

This file forms part of the P1-04 working baseline authorised by the owner’s instruction to complete the full wireframe set. Committing it records acceptance unless a later approved decision supersedes it.
