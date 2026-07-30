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
