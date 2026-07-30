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
