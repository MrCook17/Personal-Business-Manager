# 07 — Job Details

> **Project:** Personal Business Manager  
> **Phase:** P1-04 — Low-fidelity wireframes  
> **Design status:** Approved working baseline  
> **Owner:** Charlie Cook  
> **Decision date:** 29 July 2026  
> **Platform:** C# WinForms, desktop-first, dark theme  
> **Default locale:** `en-GB`, GBP  
> **Implementation rule:** Forms and controls contain no SQL; pages call application services.


## Purpose

Coordinate one job’s workflow, timer, time entries, tasks, attachments, invoices and audit history.

**Primary route:** `Work > Jobs > {Job number}`

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
Jobs / JOB-0042
JOB-0042 — Website support                    Status: Active
Acme Engineering · High · Hourly              Due: 02/08/2026
[Start timer] [Add time] [Add task] [Edit] [⋯]
```

If this job owns the active timer, show **Stop timer** and elapsed time.

## Tabs

```text
[Overview] [Time] [Tasks] [Attachments] [Invoices] [Notes] [Activity]
```

## Overview

```text
Job details                          Work summary
Customer       Acme Engineering     Tracked          12h 18m
Status         Active               Billable         10h 45m
Priority       High                 Unbilled          8h 30m
Charging       Hourly               Open tasks       4
Effective rate £45 customer         Invoiced         £900
Start / due    ...                  Outstanding      £550
```

## Child tabs

- **Time:** active timer, add manual entry, filters, correction and invoice-link state.
- **Tasks:** job-filtered tasks and quick complete/reopen.
- **Attachments:** job files.
- **Invoices:** linked invoices and eligible-unbilled-time action.
- **Notes:** explicit save.
- **Activity:** status, correction and archive history.

## Status dialog

```text
Current status: Active
New status [On hold ▾]
Reason [____________________________]  (required for reopen/cancel)
[Cancel] [Change status]
```

Rules:

- completing sets completion timestamp;
- reopening completed/cancelled work requires an audited reason;
- active timers must be resolved before completion, cancellation or archive;
- completion never invoices time automatically.

## States

| State | Presentation |
|---|---|
| Normal | Actions reflect status. |
| Archived | Banner; work-creation actions disabled. |
| Completed/cancelled | Reopen action where valid. |
| Loading | Header remains; tab loads locally. |
| Empty tab | Relevant add action. |
| Error | Tab retry. |
| Validation | Workflow explanation. |
| Concurrency | Reload before applying changes. |
| Missing | **Back to jobs**. |

## Navigation

Customer → Customer details.  
Time entry → read/correction dialog.  
Task → task dialog.  
Invoice → invoice viewer.  
Activity → audit detail.

## Scope boundaries

- Implement only the approved MVP behaviour shown here.
- Do not add speculative controls or infrastructure for deferred features.
- Later changes must update the final plan and this wireframe first.

## Approval record

This file forms part of the P1-04 working baseline authorised by the owner’s instruction to complete the full wireframe set. Committing it records acceptance unless a later approved decision supersedes it.
