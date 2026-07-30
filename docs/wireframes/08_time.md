# 08 — Time List and Active Timer

> **Project:** Personal Business Manager  
> **Phase:** P1-04 — Low-fidelity wireframes  
> **Design status:** Approved working baseline  
> **Owner:** Charlie Cook  
> **Decision date:** 29 July 2026  
> **Platform:** C# WinForms, desktop-first, dark theme  
> **Default locale:** `en-GB`, GBP  
> **Implementation rule:** Forms and controls contain no SQL; pages call application services.


## Purpose

Provide persistent timer control, manual entry, auditable correction and pageable billable/non-billable history.

**Primary route:** `Work > Time`

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
│ Time                                  [Start timer] [+ Manual entry]             │
├──────────────────────────────────────────────────────────────────────────────────┤
│ ACTIVE TIMER                                                                     │
│ Acme / JOB-0042  Started 16:52  01:18:43  Billable                              │
│ Website maintenance                  [Stop] [Switch] [Open job]                   │
├──────────────────────────────────────────────────────────────────────────────────┤
│ Search [description, job, customer____________________]                          │
│ Date [This month ▾] Customer [All ▾] Job [All ▾]                                │
│ Billable [All ▾] Method [All ▾] Invoice [All ▾] [Clear]                         │
├──────────────────────────────────────────────────────────────────────────────────┤
│ Date Start End Duration Billed Customer Job Method Description                   │
│ ...                                                                              │
│ Selected: [Open] [Correct] [Open job]                                            │
├──────────────────────────────────────────────────────────────────────────────────┤
│ 1–50 of 180                   [Previous] Page 1 [Next] Rows [50 ▾]               │
└──────────────────────────────────────────────────────────────────────────────────┘
```

## Timer workflows

No timer:

```text
No timer is running. [Start timer]
```

Forgotten timer:

```text
Running for 10h 24m.
[Continue] [Stop now] [Enter actual end] [Cancel with reason]
```

Start/switch dialog requires job, description and billable state. An existing timer offers continue, stop-and-switch or cancel.

Stop dialog shows start, end, raw duration, rounding rule and stored duration before saving. Creating the time entry and deleting the active timer is one transaction.

## Manual entry and correction

Manual entry supports start/end timestamps or date plus duration.

Correction shows original and proposed values side by side, requires a reason and recalculates duration. Entries linked to finalised invoices are read-only and point to the credit/replacement workflow.

## States

| State | Presentation |
|---|---|
| Empty | **Manual entry** or **Clear filters**. |
| Loading | Timer remains usable while history loads. |
| Error | History retry does not lose the timer. |
| Validation | Preserve entered values. |
| Timer conflict | Explicit resolution choices. |
| Invoiced entry | Read-only badge and invoice link. |
| Disconnected | Stop/start disabled until reconnection; elapsed display may continue visually. |

## Paging and navigation

Default page size: **50**.

Customer → Customer details.  
Job → Job details Time tab.  
Invoice → invoice viewer.  
Time entry → detail/correction dialog.

## Scope boundaries

- Implement only the approved MVP behaviour shown here.
- Do not add speculative controls or infrastructure for deferred features.
- Later changes must update the final plan and this wireframe first.

## Approval record

This file forms part of the P1-04 working baseline authorised by the owner’s instruction to complete the full wireframe set. Committing it records acceptance unless a later approved decision supersedes it.
