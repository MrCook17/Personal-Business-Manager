# 17 — Backups and Restore

> **Project:** Personal Business Manager  
> **Phase:** P1-04 — Low-fidelity wireframes  
> **Design status:** Approved working baseline  
> **Owner:** Charlie Cook  
> **Decision date:** 29 July 2026  
> **Platform:** C# WinForms, desktop-first, dark theme  
> **Default locale:** `en-GB`, GBP  
> **Implementation rule:** Forms and controls contain no SQL; pages call application services.


## Purpose

Show backup health, create and verify complete archives, and guide deliberate restore operations safely.

**Primary route:** `System > Backups`

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
│ Backups                           [Back up now] [Verify backup] [Restore]         │
├──────────────────────────────────────────────────────────────────────────────────┤
│ [Last backup ✓ 29/07 08:14] [Last verified ✓ 28/07] [12.4 GB free]             │
│ Automatic: first application launch each day                                    │
│ Retention: 7 daily · 4 weekly · optional monthly                                │
│ Destination: C:\...\Backups [Open folder]                                       │
├──────────────────────────────────────────────────────────────────────────────────┤
│ Date/type       Status     Size  Verified App/schema Location                    │
│ 29/07 Automatic Completed  34MB  Not yet  1.x/13     Local                      │
│ Selected: [Details] [Verify] [Restore from this backup]                          │
├──────────────────────────────────────────────────────────────────────────────────┤
│ 1–50 of 62                    [Previous] Page 1 [Next] Rows [50 ▾]               │
└──────────────────────────────────────────────────────────────────────────────────┘
```

## Back-up-now flow

Confirm inclusion of MariaDB dump, attachments, generated documents, manifest and checksums. Show progress stages: check, dump, copy, checksum, manifest, compress and atomic move.

## Verification

Check archive readability, manifest, checksums, database dump and expected file entries. Clearly distinguish archive verification from a full restore test.

## Restore wizard

1. Select and inspect a verified backup.
2. Explain current data will be replaced.
3. Reauthenticate administrator.
4. Confirm a safety backup will run first.
5. Require typed `RESTORE`.
6. Block writes, restore database/files, apply permitted migrations and validate.
7. Restart/reload and record audit result.

## States

| State | Presentation |
|---|---|
| No backups | Critical empty state + **Back up now**. |
| Loading | History loading. |
| Backup running | Step progress; duplicate actions disabled. |
| Backup failed | Warning, reference and retry. |
| Verification failed | Clear failure; no casual restore. |
| Restore running | Full-page blocking progress. |
| Restore failed | Recovery information; no false success. |
| Destination unavailable | Link to Settings; no silent fallback. |

## Paging and navigation

Default page size: **50**.

Shell backup indicator → this page.  
Audit link → filtered backup events.  
Settings link → Backup settings.

## Security

Never display passwords or process arguments containing credentials. Off-device backups should be encrypted. Reliability requires successful restore testing.

## Scope boundaries

- Implement only the approved MVP behaviour shown here.
- Do not add speculative controls or infrastructure for deferred features.
- Later changes must update the final plan and this wireframe first.

## Approval record

This file forms part of the P1-04 working baseline authorised by the owner’s instruction to complete the full wireframe set. Committing it records acceptance unless a later approved decision supersedes it.
