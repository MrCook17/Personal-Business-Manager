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
