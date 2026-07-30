# 02 — Main Application Shell

> **Project:** Personal Business Manager  
> **Phase:** P1-04 — Low-fidelity wireframes  
> **Design status:** Approved working baseline  
> **Owner:** Charlie Cook  
> **Decision date:** 29 July 2026  
> **Platform:** C# WinForms, desktop-first, dark theme  
> **Default locale:** `en-GB`, GBP  
> **Implementation rule:** Forms and controls contain no SQL; pages call application services.


## Purpose

Provide stable navigation, breadcrumbs, page hosting, timer controls, user actions, notifications and backup status.

**Primary route:** `Authenticated application host`

## Shared visual rules

- Use shared dark-theme tokens and the 4/8/16/24/32 spacing system.
- Prefer Segoe UI or Segoe UI Variable.
- Use `AutoScaleMode.Dpi`; verify at 100%, 125% and 150% scaling.
- Maintain visible keyboard focus and do not communicate status using colour alone.
- Main pages are reusable `UserControl` instances hosted by the main shell.
- Use dialogs only for focused create, edit, confirmation and correction workflows.
- Long operations are asynchronous and must not freeze the UI.


## Expanded layout

```text
┌──────────────────────────────────────────────────────────────────────────────────┐
│ ☰  Customers / Acme Engineering       Notifications  Backup ✓  Charlie Cook ▾   │
├──────────────────┬───────────────────────────────────────────────────────────────┤
│ Dashboard        │ Page heading                                                  │
│ WORK             │ ────────────────────────────────────────────────────────────  │
│ Customers        │ Main content panel                                            │
│ Jobs             │                                                               │
│ Time             │ One reusable UserControl page is hosted here.                 │
│ Tasks            │                                                               │
│ BUSINESS FINANCE │                                                               │
│ Invoices         │                                                               │
│ Expenses         │                                                               │
│ Business Reports │                                                               │
│ PERSONAL FINANCE │                                                               │
│ Accounts         │                                                               │
│ Applications     │                                                               │
│ Personal Reports │                                                               │
│ SYSTEM           │                                                               │
│ Audit History    │                                                               │
│ Backups          │                                                               │
│ Settings         │                                                               │
├──────────────────┴───────────────────────────────────────────────────────────────┤
│ TIMER  Acme / JOB-0042  01:18:43  [Stop] [Switch] [Open job]                    │
└──────────────────────────────────────────────────────────────────────────────────┘
```

## Collapsed navigation

The collapsed sidebar keeps icons, section separators, selected-state indication and tooltips. It must remain keyboard navigable.

## Header

- Left: sidebar toggle, page title and breadcrumbs.
- Right: notifications, backup indicator and user menu.
- User menu: **Lock**, **Account/Security**, **Sign out**.
- Breadcrumbs are clickable only for real, safe parent routes.

## Persistent timer strip

- Hidden when no timer exists.
- Shows customer, job, elapsed time and billable state.
- Actions: **Stop**, **Switch**, **Open job**.
- Elapsed time is calculated from stored UTC start time.
- A forgotten-timer warning adds **Review timer**.
- Timer controls remain available during page navigation.

## Page-host lifecycle

1. Check for unsaved changes.
2. Cancel obsolete page loading.
3. Dispose the outgoing page and event subscriptions.
4. Update title and breadcrumbs.
5. Load the new page asynchronously.
6. Restore supported list/filter state.
7. Move focus to the page heading or first control.

## Notifications

```text
┌───────────────────────────────────────────┐
│ Invoice finalised successfully. [Open] × │
└───────────────────────────────────────────┘
```

Warnings and errors remain until dismissed. Raw stack traces are never shown.

## Failure states

- Page failure appears inside the content panel with **Retry**.
- A database-disconnected warning persists in the header and disables writes.
- Backup failure changes the backup indicator and links to Backups.
- A content loading overlay never blocks the sidebar or timer strip unnecessarily.

## Size and scaling

- Recommended canvas: 1440 × 900.
- Minimum usable size: 1100 × 700 at 100% scaling.
- Collapse the sidebar automatically at narrow widths.
- Move secondary actions into overflow instead of hiding primary actions.

## Approved permanent sidebar

```text
Dashboard
WORK: Customers, Jobs, Time, Tasks
BUSINESS FINANCE: Invoices, Expenses, Business Reports
PERSONAL FINANCE: Accounts, Applications, Personal Reports
SYSTEM: Audit History, Backups, Settings
```

Child records open from detail pages and tabs rather than additional permanent sidebar items.

## Scope boundaries

- Implement only the approved MVP behaviour shown here.
- Do not add speculative controls or infrastructure for deferred features.
- Later changes must update the final plan and this wireframe first.

## Approval record

This file forms part of the P1-04 working baseline authorised by the owner’s instruction to complete the full wireframe set. Committing it records acceptance unless a later approved decision supersedes it.
