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
