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
