# 06 — Job List

> **Project:** Personal Business Manager  
> **Phase:** P1-04 — Low-fidelity wireframes  
> **Design status:** Approved working baseline  
> **Owner:** Charlie Cook  
> **Decision date:** 29 July 2026  
> **Platform:** C# WinForms, desktop-first, dark theme  
> **Default locale:** `en-GB`, GBP  
> **Implementation rule:** Forms and controls contain no SQL; pages call application services.


## Purpose

Find and manage jobs across customers using server-side workflow filters and direct navigation to related work.

**Primary route:** `Work > Jobs`

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
│ Jobs                                         [Export CSV] [+ Add job]            │
├──────────────────────────────────────────────────────────────────────────────────┤
│ Search [number, title, description, customer____________]                       │
│ Status [Active + Planned ▾] Priority [All ▾] Charging [All ▾]                   │
│ Customer [All ▾] Due [Any ▾] [ ] Include archived [Clear]                      │
├──────────────────────────────────────────────────────────────────────────────────┤
│ Job no. Title           Customer  Status Priority Charging Due     Tracked       │
│ JOB-42  Website support Acme      Active High     Hourly   02 Aug  12h 18m      │
│ Selected: [Open] [Edit] [Start timer] [Archive]                                 │
├──────────────────────────────────────────────────────────────────────────────────┤
│ 1–100 of 356                  [Previous] Page 1 [Next] Rows [100 ▾]              │
└──────────────────────────────────────────────────────────────────────────────────┘
```

## Filters

- Status: planned, active, on hold, completed, cancelled.
- Priority: low, normal, high, urgent.
- Charging: hourly, fixed price, mixed, non-billable.
- Customer and due-date ranges.
- Archived jobs are excluded by default.

Default view: planned, active and on-hold jobs.

## Columns

Job number, title, customer, status, priority, charging type, start, due, tracked duration, unbilled duration/value and archive state.

## Add/edit dialog

```text
Customer *        [Search/select________________ ▾]
Job number *      [JOB-____]
Title *           [______________________________]
Description       [______________________________]
Status            [Planned ▾]
Priority          [Normal ▾]
Charging type *   [Hourly ▾]
Start / due dates [...]
Estimated hours   [______]
Hourly rate       [Use inherited] [£______]
Fixed price       [£______]
Notes             [______________________________]
[Cancel] [Save job]
```

## Rules and states

- Fixed-price work requires a fixed price before invoicing.
- Hourly/mixed work displays the effective rate source.
- Due date cannot precede start date.
- Archived customers cannot normally receive new jobs.
- Starting a timer validates job state and the one-active-timer rule.
- Archiving is blocked while a timer is active.

| State | Presentation |
|---|---|
| Empty | **Add job** or **Clear filters**. |
| Loading | Grid overlay. |
| Error | Grid retry; filters retained. |
| Validation | Field messages in dialog. |
| Timer conflict | Continue current, stop and switch, or cancel. |
| Concurrency | Reload and review. |

## Paging and navigation

Default page size: **100**.

Job → Job details.  
Customer → Customer details.  
Unbilled figure → Job details Time tab filtered to eligible entries.

## Scope boundaries

- Implement only the approved MVP behaviour shown here.
- Do not add speculative controls or infrastructure for deferred features.
- Later changes must update the final plan and this wireframe first.

## Approval record

This file forms part of the P1-04 working baseline authorised by the owner’s instruction to complete the full wireframe set. Committing it records acceptance unless a later approved decision supersedes it.
