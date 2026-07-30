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
