# Personal Business Manager — P1-04 Combined Wireframe Review

> Review copy only. Use `p1_04_wireframes.zip` for the repository-ready `docs/wireframes/` structure.


---

<!-- docs/wireframes/01_login.md -->

# 01 — Login and Application Unlock

> **Project:** Personal Business Manager  
> **Phase:** P1-04 — Low-fidelity wireframes  
> **Design status:** Approved working baseline  
> **Owner:** Charlie Cook  
> **Decision date:** 29 July 2026  
> **Platform:** C# WinForms, desktop-first, dark theme  
> **Default locale:** `en-GB`, GBP  
> **Implementation rule:** Forms and controls contain no SQL; pages call application services.


## Purpose

Authenticate the local administrator, support first-run setup and unlock an inactive session without exposing database credentials.

**Primary route:** `Application startup; no sidebar route`

## Shared visual rules

- Use shared dark-theme tokens and the 4/8/16/24/32 spacing system.
- Prefer Segoe UI or Segoe UI Variable.
- Use `AutoScaleMode.Dpi`; verify at 100%, 125% and 150% scaling.
- Maintain visible keyboard focus and do not communicate status using colour alone.
- Main pages are reusable `UserControl` instances hosted by the main shell.
- Use dialogs only for focused create, edit, confirmation and correction workflows.
- Long operations are asynchronous and must not freeze the UI.


## Existing-user layout

```text
┌──────────────────────────────────────────────────────────────────────┐
│ PERSONAL BUSINESS MANAGER                           Version 1.x      │
│                                                                      │
│                  ┌────────────────────────────┐                      │
│                  │ Sign in                    │                      │
│                  │ Username [______________]  │                      │
│                  │ Password [____________] ◉  │                      │
│                  │ [ Sign in ]                │                      │
│                  │ Use recovery code          │                      │
│                  └────────────────────────────┘                      │
│ Database: Connected                 Backup: Last successful 08:14    │
└──────────────────────────────────────────────────────────────────────┘
```

Safe status indicators must never display a connection string, database password or secret path.

## First-run administrator state

```text
┌──────────────────────────────────┐
│ Create administrator             │
│ Display name [________________]   │
│ Username     [________________]   │
│ Password     [________________]   │
│ Confirm      [________________]   │
│ [ Create administrator ]         │
└──────────────────────────────────┘
```

After creation, generate a recovery code, show it once, offer **Copy**, require confirmation that it has been stored, and retain only its hash.

## Inactivity-lock state

```text
Session locked
Signed in as: Charlie Cook
Password [________________] ◉
[ Unlock ] [ Sign out ]
```

A successful unlock restores the previous page and filter state.

## Recovery-code flow

1. Enter username and recovery code.
2. Enter and confirm a new password.
3. Use a neutral failure message.
4. Mark the recovery code as used.
5. Generate and display a replacement once.

## Validation and actions

- Username and password are required.
- Disable **Sign in** while authenticating.
- Repeated failures may trigger a temporary lock.
- Allow password paste and a press-and-hold reveal action.
- `Enter` submits; `Esc` on unlock offers sign out.
- Do not reveal whether the username or password was wrong.

## Screen states

| State | Presentation |
|---|---|
| Normal | Username receives initial focus. |
| First run | Administrator-creation card. |
| Authenticating | Inputs disabled; “Signing in…” shown. |
| Invalid | Neutral rejection message. |
| Locked | Safe retry time displayed. |
| Database unavailable | **Retry connection** and **Exit**; no offline writes. |
| Recovery success | Replacement recovery code shown once. |
| Unexpected error | Safe message, correlation ID and retry. |

## Scope boundaries

- Implement only the approved MVP behaviour shown here.
- Do not add speculative controls or infrastructure for deferred features.
- Later changes must update the final plan and this wireframe first.

## Approval record

This file forms part of the P1-04 working baseline authorised by the owner’s instruction to complete the full wireframe set. Committing it records acceptance unless a later approved decision supersedes it.

---

<!-- docs/wireframes/02_main_shell.md -->

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

---

<!-- docs/wireframes/03_dashboard.md -->

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

---

<!-- docs/wireframes/04_customers.md -->

# 04 — Customer List

> **Project:** Personal Business Manager  
> **Phase:** P1-04 — Low-fidelity wireframes  
> **Design status:** Approved working baseline  
> **Owner:** Charlie Cook  
> **Decision date:** 29 July 2026  
> **Platform:** C# WinForms, desktop-first, dark theme  
> **Default locale:** `en-GB`, GBP  
> **Implementation rule:** Forms and controls contain no SQL; pages call application services.


## Purpose

Search, filter, page, create and open customers while keeping archived records available but hidden by default.

**Primary route:** `Work > Customers`

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
┌─────────────────────────────────────────────────────────────────────────────────┐
│ Customers                                   [Export CSV] [+ Add customer]       │
├─────────────────────────────────────────────────────────────────────────────────┤
│ Search [company, contact, email, phone, postcode____________]                  │
│ Status [Active ▾] Town/Postcode [___________] [Clear filters]                  │
├─────────────────────────────────────────────────────────────────────────────────┤
│ Company       Primary contact   Email      Phone    Location  Jobs Outstanding │
│ Acme Eng.     Alex Smith        a@...      ...      Poole     3    £550        │
│                                                                                 │
│ Selected: [Open] [Edit] [Archive]                                               │
├─────────────────────────────────────────────────────────────────────────────────┤
│ 1–100 of 243                    [Previous] Page 1 [Next] Rows [100 ▾]           │
└─────────────────────────────────────────────────────────────────────────────────┘
```

## Filters and ordering

Search covers company/display name, contact, email, phone and postcode.

Status values:

```text
Active
Archived
All
```

Default order: company name ascending, then `record_id`.

## Grid columns

| Column | Purpose |
|---|---|
| Company | Primary identity and detail link. |
| Primary contact | Active primary contact or “Not set”. |
| Email/phone | Preferred safe contact fields. |
| Location | Town/city and postcode. |
| Active jobs | Opens the customer Jobs tab. |
| Outstanding | Opens outstanding customer invoices. |
| Status | Text plus badge. |
| Last activity | Most recent safe activity time. |

## Main actions

- **Add customer** opens a dialog that can save the customer, first contact and first address in one transaction.
- **Archive** explains that history remains and blocks inappropriate new work.
- Archived selection changes the action to **Restore**.
- **Export CSV** exports the current filtered set.

## Add dialog

```text
Company/display name * [____________________________]
Default hourly rate    [£________]
Payment terms          [30 days ▾]
VAT treatment          [Use application default ▾]
Invoice delivery       [Use application default ▾]
Notes                  [____________________________]

Primary contact
Name * [__________] Email [__________] Phone [__________]

First address
Type [Billing ▾] Address fields...

[Cancel] [Save customer]
```

## States

| State | Presentation |
|---|---|
| Empty active list | **Add customer** and optional **Show archived**. |
| Empty search | **Clear filters**. |
| Loading | Grid overlay; filters retained. |
| Error | Grid-level retry. |
| Validation | Field messages; entered contact/address retained. |
| Concurrency | Reload/review instead of overwrite. |
| Archived | Clear label; history still navigable. |

## Paging

Default page size: **100**.

## Navigation

- Company → Customer details.
- Jobs count → Customer details Jobs tab.
- Outstanding → Customer details Invoices tab.
- Breadcrumb restoration preserves the prior list filters.

## Scope boundaries

- Implement only the approved MVP behaviour shown here.
- Do not add speculative controls or infrastructure for deferred features.
- Later changes must update the final plan and this wireframe first.

## Approval record

This file forms part of the P1-04 working baseline authorised by the owner’s instruction to complete the full wireframe set. Committing it records acceptance unless a later approved decision supersedes it.

---

<!-- docs/wireframes/05_customer_details.md -->

# 05 — Customer Details

> **Project:** Personal Business Manager  
> **Phase:** P1-04 — Low-fidelity wireframes  
> **Design status:** Approved working baseline  
> **Owner:** Charlie Cook  
> **Decision date:** 29 July 2026  
> **Platform:** C# WinForms, desktop-first, dark theme  
> **Default locale:** `en-GB`, GBP  
> **Implementation rule:** Forms and controls contain no SQL; pages call application services.


## Purpose

Present the full customer record through focused tabs and preserve navigation to contacts, addresses, jobs, invoices, attachments and activity.

**Primary route:** `Work > Customers > {Customer}`

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
Customers / Acme Engineering
Acme Engineering                    Status: Active
Primary contact: Alex Smith         [Edit] [New job] [New invoice] [⋯]
```

Overflow: Archive/Restore, export summary and copy reference.

For an archived customer, show a persistent banner and disable **New job** until restoration.

## Tabs

```text
[Overview] [Contacts] [Addresses] [Jobs] [Invoices] [Attachments] [Notes] [Activity]
```

## Overview

```text
Customer defaults                    Summary
Hourly rate       £45.00             Active jobs       3
Payment terms     30 days            Unbilled time    12h
VAT treatment     Standard           Outstanding     £550
Delivery          Email              Last activity   29 Jul

Primary contact                    Default billing address
Alex Smith ...                     ...
[Edit]                             [Edit]
```

## Contacts

Toolbar: **Add contact**, search, **Include archived**.

```text
Primary Name       Job title Email Phone Mobile Status
Yes     Alex Smith Manager   ...   ...   ...    Active
```

Actions: edit, make primary, archive/restore. Making a new primary contact updates both records transactionally.

## Addresses

Toolbar: **Add address**, type filter, **Include archived**.

```text
Default Type       Recipient     Town  Postcode Status
Yes     Billing    Accounts      Poole BH...    Active
```

Only one active default per address type is allowed.

## Jobs and invoices

The Jobs and Invoices tabs host the same paged list patterns as their main screens, prefiltered to the customer.

Jobs actions: **Add job**, Open.  
Invoices actions: **New invoice**, Open, Record payment where valid.

## Attachments, notes and activity

- Attachments: add, open, save copy and retain referential history.
- Notes: explicit edit/save with unsaved-change protection.
- Activity: read-only customer audit events with a link to full Audit History.

## States

| State | Presentation |
|---|---|
| Normal | Header and selected tab. |
| Archived | Persistent banner; creation actions disabled; Restore available. |
| Loading | Identity/header remains; tab loads locally. |
| Empty tab | Tab-specific add action. |
| Error | Tab-level retry. |
| Validation | Dialog field messages. |
| Concurrency | Reload and review instead of overwrite. |
| Missing record | **Back to customers**. |

## Navigation

Job → Job details.  
Invoice → Invoice viewer.  
Activity item → audit detail.  
Breadcrumb returns to the prior customer-list state.

## Scope boundaries

- Implement only the approved MVP behaviour shown here.
- Do not add speculative controls or infrastructure for deferred features.
- Later changes must update the final plan and this wireframe first.

## Approval record

This file forms part of the P1-04 working baseline authorised by the owner’s instruction to complete the full wireframe set. Committing it records acceptance unless a later approved decision supersedes it.

---

<!-- docs/wireframes/06_jobs.md -->

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

---

<!-- docs/wireframes/07_job_details.md -->

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

---

<!-- docs/wireframes/08_time.md -->

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

---

<!-- docs/wireframes/09_tasks.md -->

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

---

<!-- docs/wireframes/10_invoices.md -->

# 10 — Invoice List

> **Project:** Personal Business Manager  
> **Phase:** P1-04 — Low-fidelity wireframes  
> **Design status:** Approved working baseline  
> **Owner:** Charlie Cook  
> **Decision date:** 29 July 2026  
> **Platform:** C# WinForms, desktop-first, dark theme  
> **Default locale:** `en-GB`, GBP  
> **Implementation rule:** Forms and controls contain no SQL; pages call application services.


## Purpose

Search, filter and open invoices and credit notes while separating editable drafts, finalised documents, payments and derived overdue state.

**Primary route:** `Business Finance > Invoices`

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
│ Invoices                  [Export CSV] [+ New invoice] [+ Credit note]           │
├──────────────────────────────────────────────────────────────────────────────────┤
│ [Draft] [Finalised] [Sent] [Outstanding] [Paid] [Cancelled] [Credit notes] [All]│
│ Search [number, customer, notes/reference________________]                       │
│ Date [This year ▾] Customer [All ▾] Due [Any ▾] Type [All ▾] [Clear]           │
├──────────────────────────────────────────────────────────────────────────────────┤
│ Number Type    Customer Date      Due       Status Gross Paid Outstanding        │
│ INV-102 Invoice Acme    15/07/26 14/08/26  Sent   £900  £350 £550               │
│ Selected: [Open] [PDF] [Record payment] [Create credit note]                     │
├──────────────────────────────────────────────────────────────────────────────────┤
│ 1–50 of 201                    [Previous] Page 1 [Next] Rows [50 ▾]              │
└──────────────────────────────────────────────────────────────────────────────────┘
```

## Views

- Draft: editable drafts.
- Finalised: numbered and financially locked.
- Sent: delivered but not fully settled/credited.
- Outstanding: derived outstanding amount above zero.
- Paid: zero outstanding due to payments.
- Cancelled: abandoned drafts.
- Credit notes: document type credit note.
- All.

Overdue is a derived warning inside Outstanding, not a stored status.

## Actions

- New invoice.
- Create credit note from an eligible original.
- Open draft editor or finalised viewer.
- Open/regenerate PDF.
- Record payment.
- Export the filtered list.

## Payment dialog

```text
Invoice: INV-102
Outstanding       £550.00
Payment date *    [29/07/2026]
Amount *          [£________]
Method *          [Bank transfer ▾]
Received into     [Business account ▾]
Reference         [________________]
Notes             [________________]
[Cancel] [Record payment]
```

An overpayment requires explicit confirmation showing the excess.

## States

| State | Presentation |
|---|---|
| Empty | View-specific message; Draft offers **New invoice**. |
| Loading | Grid loading; view tabs remain. |
| Error | Retry without clearing filters. |
| Validation | Payment/credit field messages. |
| PDF failure | Document remains valid; **Regenerate PDF**. |
| Concurrency | Reload totals before write. |
| Cancelled/credited | Read-only historical state. |

## Paging and navigation

Default page size: **50**.

Document number → editor/viewer.  
Customer → Customer details Invoices tab.  
Dashboard outstanding cards → filtered Outstanding view.

Invoices are not archived or hard-deleted.

## Scope boundaries

- Implement only the approved MVP behaviour shown here.
- Do not add speculative controls or infrastructure for deferred features.
- Later changes must update the final plan and this wireframe first.

## Approval record

This file forms part of the P1-04 working baseline authorised by the owner’s instruction to complete the full wireframe set. Committing it records acceptance unless a later approved decision supersedes it.

---

<!-- docs/wireframes/11_invoice_editor.md -->

# 11 — Invoice Editor and Viewer

> **Project:** Personal Business Manager  
> **Phase:** P1-04 — Low-fidelity wireframes  
> **Design status:** Approved working baseline  
> **Owner:** Charlie Cook  
> **Decision date:** 29 July 2026  
> **Platform:** C# WinForms, desktop-first, dark theme  
> **Default locale:** `en-GB`, GBP  
> **Implementation rule:** Forms and controls contain no SQL; pages call application services.


## Purpose

Create validated drafts, finalise atomically, display immutable finalised documents, generate PDFs and support payments and credit notes.

**Primary route:** `Business Finance > Invoices > {Draft or invoice number}`

## Shared visual rules

- Use shared dark-theme tokens and the 4/8/16/24/32 spacing system.
- Prefer Segoe UI or Segoe UI Variable.
- Use `AutoScaleMode.Dpi`; verify at 100%, 125% and 150% scaling.
- Maintain visible keyboard focus and do not communicate status using colour alone.
- Main pages are reusable `UserControl` instances hosted by the main shell.
- Use dialogs only for focused create, edit, confirmation and correction workflows.
- Long operations are asynchronous and must not freeze the UI.


## Draft editor

```text
┌──────────────────────────────────────────────────────────────────────────────────┐
│ Invoices / Draft invoice                   [Save draft] [Finalise] [⋯]           │
├──────────────────────────────────────────────────────────────────────────────────┤
│ Customer * [Search customer________________ ▾] Status: Draft                     │
│ Invoice date [29/07/26] Due [28/08/26] Prices [Ex VAT ▾] Currency GBP           │
│ Billing preview [Use current customer details]                                  │
│ Customer notes [___________________________________________________________]    │
│ Internal notes [___________________________________________________________]    │
├──────────────────────────────────────────────────────────────────────────────────┤
│ LINES                                                                            │
│ [Add time] [Fixed price] [Manual] [Recharge expense] [Adjustment]               │
│ # Type Description          Qty    Rate     Disc   Net   VAT  Gross              │
│ 1 Time Website work        10.00h £45.00   None   £450  £90  £540               │
│                                                        [Edit] [Remove]           │
├──────────────────────────────────────────────────────────────────────────────────┤
│ Payment instructions [...]                    Net £450 VAT £90 Gross £540         │
└──────────────────────────────────────────────────────────────────────────────────┘
```

## Time selection

Show only eligible, uninvoiced billable time. Include date, job, description, rounded duration, effective rate and calculated amount. The service revalidates all selections during finalisation.

## Line dialog

```text
Type [Manual ▾]
Description * [____________________________]
Quantity * [1.0000] Unit rate * [£0.0000]
Discount [None ▾] [value]
VAT [Standard 20% ▾]
Calculated: Net ... VAT ... Gross ...
[Cancel] [Save line]
```

The same Core calculation service drives preview and finalisation.

## Finalisation confirmation

```text
Finalise invoice?
This allocates the next number and locks financial content.

Customer: Acme Engineering
Net £450.00  VAT £90.00  Gross £540.00
Selected time entries: 4

[Continue editing] [Finalise invoice]
```

Repeated clicks cannot create duplicate numbers or time links.

## Finalised viewer

Read-only billing snapshot, dates, lines, totals, payments, credits, PDF details and audit link.

Actions:

```text
Open PDF
Regenerate PDF
Record payment
Create credit note
Mark sent where valid
```

## Credit-note mode

- References the original invoice and original lines.
- Limits credit to remaining uncredited value.
- Does not reattach time entries.
- Uses the credit-note number sequence.
- Fully credited original invoices show `credited`.

## Validation summary

```text
Invoice cannot be finalised:
• Add at least one valid line.
• Select a billing address.
• JOB-0042 has no effective hourly rate.
[Go to first issue]
```

## States

| State | Presentation |
|---|---|
| New draft | Empty line state with add actions. |
| Saved draft | Editable with last-saved time. |
| Loading | Editor overlay. |
| Validation | Summary and field/line messages. |
| Finalising | All duplicate actions disabled. |
| Finalised | Immutable viewer. |
| Concurrent edit | Reload and review. |
| Missing source | Explain affected line and require correction. |
| PDF failed | Invoice remains valid; regenerate action. |

## Unsaved changes

Prompt with **Keep editing**, **Discard**, **Save draft**.

## Navigation

Customer → Customer details.  
Job/time source → related details.  
Payment/credit → linked workflow.  
Audit → filtered Audit History.

## Scope boundaries

- Implement only the approved MVP behaviour shown here.
- Do not add speculative controls or infrastructure for deferred features.
- Later changes must update the final plan and this wireframe first.

## Approval record

This file forms part of the P1-04 working baseline authorised by the owner’s instruction to complete the full wireframe set. Committing it records acceptance unless a later approved decision supersedes it.

---

<!-- docs/wireframes/12_business_finance.md -->

# 12 — Business Finance Workspace

> **Project:** Personal Business Manager  
> **Phase:** P1-04 — Low-fidelity wireframes  
> **Design status:** Approved working baseline  
> **Owner:** Charlie Cook  
> **Decision date:** 29 July 2026  
> **Platform:** C# WinForms, desktop-first, dark theme  
> **Default locale:** `en-GB`, GBP  
> **Implementation rule:** Forms and controls contain no SQL; pages call application services.


## Purpose

Define coordinated expense, payment, business-account and management-report screens while keeping the concise sidebar.

**Primary route:** `Business Finance > Expenses or Business Reports`

## Shared visual rules

- Use shared dark-theme tokens and the 4/8/16/24/32 spacing system.
- Prefer Segoe UI or Segoe UI Variable.
- Use `AutoScaleMode.Dpi`; verify at 100%, 125% and 150% scaling.
- Maintain visible keyboard focus and do not communicate status using colour alone.
- Main pages are reusable `UserControl` instances hosted by the main shell.
- Use dialogs only for focused create, edit, confirmation and correction workflows.
- Long operations are asynchronous and must not freeze the UI.


## Route model

The permanent sidebar uses:

```text
Invoices
Expenses
Business Reports
```

The shared workspace uses:

```text
[Overview] [Payments] [Expenses] [Business accounts] [Reports]
```

Opening Expenses selects Expenses. Opening Business Reports selects Overview/Reports.

## Overview

```text
Date range [This month ▾] [Refresh]
[Invoiced £6,400] [Received £5,100] [Expenses £1,340]
[Invoiced profit est. £5,060] [Cash profit est. £3,760]
[Output VAT £1,080] [Input VAT £120] [VAT estimate £960]
[Tax reserve estimate £1,012]

Planning estimates only; not accounting or tax advice.
[Open source rows] [Export PDF] [Export CSV]
```

Charts may be added only with accessible table/source-row equivalents.

## Payments

Paged list:

```text
Date Invoice Customer Amount Method Account Reference Reversed
```

Filters: date, customer, invoice, method, receiving account and reversed state.

Actions: open invoice, reverse with reason, export CSV. Payments are reversed, never edited/deleted.

## Expenses

```text
Expenses                              [Export CSV] [+ Add expense]
Search [supplier, description, reference________]
Date [This month ▾] Category [All ▾] Account [All ▾]
Tax estimate [All ▾] [ ] Include archived

Date Supplier Category Description Net VAT Gross Paid from Receipt
```

Default page size: **50**.

Dialog fields: date, supplier, category, description, net/VAT/gross, business account, payment method, reference, estimated deductibility, notes and receipt.

## Business accounts

Only `account_scope_code = business`.

```text
Account Provider Type Status Current balance Last updated
```

Actions: add account, open details, update balance.

## Reports

Selectors include revenue, received income, ageing, expenses, profit estimates, customer/job revenue, hours, VAT and tax-reserve estimate.

Controls: date range, relevant entity filters, gross/net where appropriate, run, CSV and PDF.

Every output includes title, filters, generated time and `Business` scope.

## States

| State | Presentation |
|---|---|
| Empty list | Relevant add/navigation action. |
| Loading report | Cancellable result-area progress. |
| Error | Section retry and correlation ID. |
| Validation | Expense/payment/reversal messages. |
| Estimate unavailable | Explain missing data/configuration; no misleading zero. |
| Archived expense | Hidden by default and labelled. |
| Reversed payment | Retained with timestamp and reason. |

## Navigation

Invoice/payment → Invoice viewer.  
Customer/job report row → details.  
Expense → view/edit dialog.  
Business account → Account details.  
Report source rows → underlying filtered list.

## Excluded from MVP

No general ledger, payroll, bank reconciliation, Open Banking, automatic banking login, or automatic tax/VAT submission.

## Scope boundaries

- Implement only the approved MVP behaviour shown here.
- Do not add speculative controls or infrastructure for deferred features.
- Later changes must update the final plan and this wireframe first.

## Approval record

This file forms part of the P1-04 working baseline authorised by the owner’s instruction to complete the full wireframe set. Committing it records acceptance unless a later approved decision supersedes it.

---

<!-- docs/wireframes/13_personal_accounts.md -->

# 13 — Personal Account List

> **Project:** Personal Business Manager  
> **Phase:** P1-04 — Low-fidelity wireframes  
> **Design status:** Approved working baseline  
> **Owner:** Charlie Cook  
> **Decision date:** 29 July 2026  
> **Platform:** C# WinForms, desktop-first, dark theme  
> **Default locale:** `en-GB`, GBP  
> **Implementation rule:** Forms and controls contain no SQL; pages call application services.


## Purpose

Show personal assets, liabilities and net worth with safe account creation and manual balance updates.

**Primary route:** `Personal Finance > Accounts`

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
│ Personal accounts                         [Export CSV] [+ Add account]           │
├──────────────────────────────────────────────────────────────────────────────────┤
│ [Assets £31,250] [Liabilities £1,100] [Net worth £30,150]                       │
│ [Current £2,250] [Savings £18,000] [ISAs £11,000]                              │
│ Estimates use latest recorded balances.                                         │
├──────────────────────────────────────────────────────────────────────────────────┤
│ Search [provider, name, last four____________]                                  │
│ Type [All ▾] Class [All ▾] Status [Open ▾] Updates [All ▾]                     │
│ [ ] Show hidden [ ] Include archived [Clear]                                    │
├──────────────────────────────────────────────────────────────────────────────────┤
│ Provider Account      Type       Class     Status Balance Updated                │
│ Bank A   Everyday 1234 Current   Asset     Open   £1,250  Today                  │
│ Selected: [Open] [Update balance] [Edit] [Hide]                                 │
├──────────────────────────────────────────────────────────────────────────────────┤
│ 1–100 of 12                   [Previous] Page 1 [Next] Rows [100 ▾]              │
└──────────────────────────────────────────────────────────────────────────────────┘
```

The route is fixed to personal scope. Business accounts never appear here.

## Add account

Fields: provider, account name/type, last four, status, opening/current and available balances, credit limit where relevant, interest rate/type, key dates, tax wrapper and notes.

## Update balance

```text
Account: Everyday 1234
Current recorded balance £1,250.00
Balance at * [29/07/2026 18:10]
New balance * [£________]
Available [£________]
Source [Manual ▾]
Notes [________________]
[Cancel] [Save balance]
```

One transaction creates the snapshot and updates current values.

## Semantics

- Net worth = assets minus liabilities.
- Negative asset balances reduce net worth.
- Negative liability balances represent credit.
- Contributions do not automatically change balances.

## States

| State | Presentation |
|---|---|
| Empty | **Add account**. |
| Loading | Cards and list load independently. |
| Error | Retry; failed values are not shown as zero. |
| Validation | Account-type-specific messages. |
| Stale | Text/icon based on update age. |
| Hidden | Excluded unless requested. |
| Archived | Excluded by default. |
| Scope mismatch | Data-integrity error; never show business data. |

## Paging and navigation

Default page size: **100**.

Account → Account details.  
Summary card → same list with filter.  
Dashboard → matching filtered account view.

## Security

Never request or store banking passwords, PINs, full account/card numbers, security codes or authentication secrets.

## Scope boundaries

- Implement only the approved MVP behaviour shown here.
- Do not add speculative controls or infrastructure for deferred features.
- Later changes must update the final plan and this wireframe first.

## Approval record

This file forms part of the P1-04 working baseline authorised by the owner’s instruction to complete the full wireframe set. Committing it records acceptance unless a later approved decision supersedes it.

---

<!-- docs/wireframes/14_account_details.md -->

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

---

<!-- docs/wireframes/15_applications.md -->

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

---

<!-- docs/wireframes/16_audit_history.md -->

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

---

<!-- docs/wireframes/17_backups.md -->

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

---

<!-- docs/wireframes/18_settings.md -->

# 18 — Settings

> **Project:** Personal Business Manager  
> **Phase:** P1-04 — Low-fidelity wireframes  
> **Design status:** Approved working baseline  
> **Owner:** Charlie Cook  
> **Decision date:** 29 July 2026  
> **Platform:** C# WinForms, desktop-first, dark theme  
> **Default locale:** `en-GB`, GBP  
> **Implementation rule:** Forms and controls contain no SQL; pages call application services.


## Purpose

Centralise validated configuration for general behaviour, invoicing, VAT, time, security, backups and protected database access.

**Primary route:** `System > Settings`

## Shared visual rules

- Use shared dark-theme tokens and the 4/8/16/24/32 spacing system.
- Prefer Segoe UI or Segoe UI Variable.
- Use `AutoScaleMode.Dpi`; verify at 100%, 125% and 150% scaling.
- Maintain visible keyboard focus and do not communicate status using colour alone.
- Main pages are reusable `UserControl` instances hosted by the main shell.
- Use dialogs only for focused create, edit, confirmation and correction workflows.
- Long operations are asynchronous and must not freeze the UI.


## Layout

```text
┌──────────────────────────────────────────────────────────────────────────────────┐
│ Settings                                      [Discard] [Save changes]           │
├─────────────────────┬────────────────────────────────────────────────────────────┤
│ General             │ Selected settings category                               │
│ Business details    │                                                            │
│ Invoicing and VAT   │ Label [control]                                            │
│ Time tracking       │ Label [control]                                            │
│ Security            │ ...                                                        │
│ Backups             │                                                            │
│ Database            │                                                            │
│ About               │                                                            │
└─────────────────────┴────────────────────────────────────────────────────────────┘
```

No light-theme, accent-colour or custom-dashboard settings appear. Dark theme is fixed for the MVP.

## Categories

### General

Application display name, approved page sizes, default payment terms, default hourly rate, system/local time-zone display and forgotten-timer threshold.

Locale defaults to `en-GB`; currency defaults to GBP.

### Business details

Business/trading name, address, email, telephone, VAT number and invoice payment-instruction text. Never store banking login credentials.

### Invoicing and VAT

```text
VAT registered [✓]
Default VAT rate [20.0000%]
Prices entered [Exclusive ▾]
Payment terms [30]
Invoice prefix [INV-]
Credit-note prefix [CN-]
Default notes/instructions [...]
```

Prefix/sequence changes require validation and warning. They must not overwrite active sequence values.

### Time tracking

Default billable state, default approved rounding rule, forgotten-timer threshold and manual date-plus-duration option.

### Security

Inactivity lock, password change, recovery-code replacement and read-only failed-login policy summary. Recovery replacement requires reauthentication and shows the new code once.

### Backups

Folder, automatic first-launch-per-day toggle, daily/weekly/monthly retention, **Back up now** and link to Backups.

### Database

```text
Status             Connected
Server version     Safe value
Database           personal_business_manager
Runtime identity   personal_business_app@localhost
Credential storage Windows protected storage
[Test connection] [Update protected credential]
```

Never show password/full connection string. Migration execution does not belong in ordinary settings.

### About

Application/schema/.NET versions, licence/reference notices and safe log-folder link.

## Save behaviour

- Track dirty fields.
- Validate every category before writing.
- Show summary and field-level messages.
- Save related changes transactionally where practical.
- Audit important security and numbering changes.
- Show restart notice only when required.
- Prevent duplicate saves.

## States

| State | Presentation |
|---|---|
| Normal | Save disabled until changed. |
| Loading | Category list remains; panel overlay. |
| Validation | Summary and linked field messages. |
| Saving | Save/Discard disabled. |
| Success | Non-blocking notification. |
| Error | Preserve unsaved values; retry. |
| Concurrency | Reload and allow reapplication. |
| Database unavailable | Database section disconnected; safe settings remain viewable. |
| Unsaved navigation | Save, discard or continue editing. |

## Keyboard and Phase 3 readiness

Category list is keyboard navigable, labels bind to controls and `Ctrl+S` saves when valid.

This structure is sufficient for Phase 3 login, recovery, session lock, settings service and audit implementation without inventing new page layout.

## Scope boundaries

- Implement only the approved MVP behaviour shown here.
- Do not add speculative controls or infrastructure for deferred features.
- Later changes must update the final plan and this wireframe first.

## Approval record

This file forms part of the P1-04 working baseline authorised by the owner’s instruction to complete the full wireframe set. Committing it records acceptance unless a later approved decision supersedes it.
