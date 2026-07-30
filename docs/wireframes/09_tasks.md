# 09 — Task List

> **Project:** Personal Business Manager  
> **Phase:** P1-04 — Low-fidelity wireframes  
> **Design status:** Approved working baseline  
> **Owner:** Charlie Cook  
> **Decision date:** 29 July 2026  
> **Platform:** C# WinForms, desktop-first, dark theme  
> **Default locale:** `en-GB`, GBP  
> **Implementation rule:** Forms and controls contain no SQL; pages call application services.


## Purpose

Manage general and job-linked tasks through due-date views while deferring recurring tasks and calendar presentation.

**Primary route:** `Work > Tasks`

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
│ Tasks                                                  [+ Add task]             │
├──────────────────────────────────────────────────────────────────────────────────┤
│ [Overdue 3] [Today 5] [Upcoming] [No due date] [Completed] [All]                │
│ Search [title, notes, job____________] Status [Active ▾] Priority [All ▾]        │
│ Job [All ▾] [ ] Include archived [Clear]                                         │
├──────────────────────────────────────────────────────────────────────────────────┤
│ ✓ Task              Job       Status      Priority Due     Updated               │
│ □ Send design proof JOB-0042  In progress High     Today   17:40                 │
│ Selected: [Open] [Complete] [Open job] [Archive]                                 │
├──────────────────────────────────────────────────────────────────────────────────┤
│ 1–100 of 125                  [Previous] Page 1 [Next] Rows [100 ▾]              │
└──────────────────────────────────────────────────────────────────────────────────┘
```

No calendar view is included in the MVP. A later calendar view may consume the same due-date data.

## Views

- Overdue: before the current local date and not complete/cancelled.
- Today.
- Upcoming.
- No due date.
- Completed.
- All.

## Add/edit dialog

```text
Task title * [____________________________]
Linked job   [None / search____________ ▾]
Status       [Not started ▾]
Priority     [Normal ▾]
Due date     [None / dd/mm/yyyy]
Notes        [____________________________]
[Cancel] [Save task]
```

A task without a job is a general business task.

## Actions and states

Completing sets the completion timestamp. Reopening clears it. Archive is separate from status.

| State | Presentation |
|---|---|
| Empty view | View-specific positive message. |
| Empty overall | **Add task**. |
| Loading | View tabs stay visible. |
| Error | Grid retry. |
| Validation | Required title/date messages. |
| Linked job unavailable | Historical task remains visible. |
| Concurrency | Reload before edit/completion. |
| Archived | Hidden by default and labelled when included. |

## Paging and navigation

Default page size: **100**.

Task → edit/details dialog.  
Job → Job details Tasks tab.  
Dashboard cards open the matching view.

## Scope boundaries

- Implement only the approved MVP behaviour shown here.
- Do not add speculative controls or infrastructure for deferred features.
- Later changes must update the final plan and this wireframe first.

## Approval record

This file forms part of the P1-04 working baseline authorised by the owner’s instruction to complete the full wireframe set. Committing it records acceptance unless a later approved decision supersedes it.
