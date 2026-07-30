# 16 — Audit History

> **Project:** Personal Business Manager  
> **Phase:** P1-04 — Low-fidelity wireframes  
> **Design status:** Approved working baseline  
> **Owner:** Charlie Cook  
> **Decision date:** 29 July 2026  
> **Platform:** C# WinForms, desktop-first, dark theme  
> **Default locale:** `en-GB`, GBP  
> **Implementation rule:** Forms and controls contain no SQL; pages call application services.


## Purpose

Provide an append-only, searchable record of important application, financial, security and correction events.

**Primary route:** `System > Audit History`

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
│ Audit history                                               [Export CSV]         │
├──────────────────────────────────────────────────────────────────────────────────┤
│ Search [record, reason, correlation ID____________]                            │
│ Date [Last 30 days ▾] User [All ▾] Entity [All ▾] Action [All ▾]              │
│ [Clear filters]                                                                  │
├──────────────────────────────────────────────────────────────────────────────────┤
│ Occurred           User    Action            Entity          Record Summary       │
│ 29/07 18:02:11     Charlie Balance updated   Financial acct  42     ...           │
│ 29/07 17:55:03     System  Backup completed  Backup          17     ...           │
│ Selected: [View details] [Open related record]                                   │
├──────────────────────────────────────────────────────────────────────────────────┤
│ 1–50 of 2,180                 [Previous] Page 1 [Next] Rows [50 ▾]               │
└──────────────────────────────────────────────────────────────────────────────────┘
```

System events display `System`; they do not invent a user identity.

## Detail view

```text
Occurred:       local and UTC timestamp
User:           Charlie Cook
Action:         Balance updated
Entity/record:  Financial account / 42
Reason:         Monthly update
Correlation ID: ...

Before
Current balance £1,100.00

After
Current balance £1,250.00
[Open related account] [Copy correlation ID] [Close]
```

## Filters and actions

Date/time, user, entity, action, record ID, correlation ID and safe reason/summary search.

Default order: newest first, then record ID descending.

Actions: view detail, open related record, copy correlation ID and export current filter. No edit/delete/archive.

## States

| State | Presentation |
|---|---|
| Empty | **Clear filters**. |
| Loading | Grid loading. |
| Error | Retry and safe correlation ID. |
| Missing related record | Keep event; disable link. |
| Invalid legacy detail | Safe unavailable/fallback message. |
| Large detail | Collapsible before/after sections. |

## Paging

Default page size: **50**.

## Security

Audit content must omit/redact passwords, hashes, recovery codes, connection strings, full financial identifiers and authentication tokens before storage.

## Scope boundaries

- Implement only the approved MVP behaviour shown here.
- Do not add speculative controls or infrastructure for deferred features.
- Later changes must update the final plan and this wireframe first.

## Approval record

This file forms part of the P1-04 working baseline authorised by the owner’s instruction to complete the full wireframe set. Committing it records acceptance unless a later approved decision supersedes it.
