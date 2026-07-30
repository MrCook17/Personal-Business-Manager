# Phase 1 Approval

**Project:** Personal Business Manager  
**Decision:** P1-01 — Formally freeze and approve the MVP scope  
**Date:** 29 July 2026  
**Owner:** Charlie Cook  
**Status:** Approved

---

## 1. Approval summary

- **MVP scope approved:** Yes
- **Deferred features approved:** Yes, subject to the reclassifications in this document
- **Optional features approved:** No — the former optional items are excluded, except for the calendar task view
- **Excluded first-version features approved:** Yes, subject to the reclassifications in this document
- **Final recommended first version approved:** Yes, where it does not conflict with the decisions below
- **Navigation approved:** Not covered by this decision; to be approved separately
- **Schema approved:** Not covered by this decision; to be approved under P1-03
- **Migration order approved:** Not covered by this decision; to be approved under P1-07

---

## 2. Section 6.1 — Essential MVP features

All features currently listed in Section 6.1, **Essential MVP features**, are approved for inclusion in the first production-capable version.

This includes the essential platform, reliability, business operations, business finance and personal finance features defined in the development plan.

No Section 6.1 feature is removed or deferred by this approval.

---

## 3. Section 6.2 — Useful later

All features currently listed in Section 6.2 are approved as deferred features.

They are **not included in the first version**, but the application should be designed so they can be added later without requiring an unnecessary rewrite of the core architecture or data model.

The following additional features are moved into Section 6.2:

- Calendar task view.
- Bank reconciliation.
- Open Banking connections.
- Automatic banking login.
- Automatic tax return submission.

### Future-compatibility rule

The first version does not need to implement these features, create unused screens for them, or add speculative infrastructure solely for them.

However, architectural decisions should avoid unnecessarily preventing their future implementation. In particular:

- Business logic should remain outside WinForms forms and controls.
- Service and repository boundaries should remain replaceable.
- Financial-account data should remain structured and extensible.
- Task data should support a future calendar presentation.
- Database migrations should allow later additions without editing released migrations.
- Sensitive banking credentials must not be stored by the first version.

Future compatibility does not override security, simplicity or the approved MVP scope.

---

## 4. Section 6.3 — Optional features

The features previously listed in Section 6.3 are not approved for the planned application scope.

They should be treated as excluded unless a future approved plan change explicitly adds them.

The excluded items are:

- Light theme.
- User-selectable accent colour.
- Custom dashboard layout.
- Multiple invoice designs.
- Favourite jobs and customers.
- Outlook or Google Calendar integration.
- Windows Hello unlocking.
- Custom report designer.
- Provider logos.

The **calendar task view** is the only exception. It is moved to Section 6.2 as a possible later feature.

These excluded items should not influence the current architecture, schema, wireframes, controls or development roadmap.

---

## 5. Section 6.4 — Avoid in the first version

The following Section 6.4 items are reclassified as useful later and moved to Section 6.2:

- Bank reconciliation.
- Open Banking connections.
- Automatic banking login.
- Automatic tax return submission.

They remain excluded from the first version.

All other features previously listed in Section 6.4 are outside the approved scope and may be ignored unless a future approved change explicitly introduces them.

This includes:

- Full double-entry bookkeeping.
- General ledger and journal entries.
- Payroll.
- Automatic VAT submission.
- Investment trading.
- Live market-price feeds.
- Customer portal.
- Multi-company SaaS architecture.
- Stock control.
- Purchase-order system.
- Workflow designer.
- Plug-in system.
- Microservices.
- Event sourcing.
- Message queues.
- Artificial-intelligence financial categorisation.

No first-version implementation, schema preparation, user interface, placeholder module or speculative abstraction is required for these excluded features.

---

## 6. Section 32 — Final recommended first version

Section 32 is approved as the recommended first production-capable version, provided it is interpreted consistently with the decisions in this document.

Where Section 32 conflicts with this approval, this approval takes precedence.

### Section 32 interpretation

The first version should include all approved Section 6.1 MVP features.

The first version should not include:

- Features classified as useful later.
- Features excluded from Section 6.3.
- Features excluded from Section 6.4.

The following are specifically deferred despite being considered potentially useful in the future:

- Calendar task view.
- Bank reconciliation.
- Open Banking connections.
- Automatic banking login.
- Automatic tax return submission.

References in Section 32 to delaying bank feeds, remote access, multiple users, light theme and other later features remain approved.

---

## 7. Scope control rules

The following rules apply after this approval:

1. Phase 1 and Phase 2 work must not silently add deferred or excluded features.
2. Deferred features may be considered when choosing clean extension points, but they must not expand the first-version workload.
3. Excluded features must not be used to justify additional tables, controls, services or dependencies.
4. Any future scope change must be recorded in the main development plan and its change log.
5. Released database migrations must not be edited to accommodate future scope changes; new migrations must be added.
6. Security rules take precedence over future-compatibility considerations.
7. Automatic banking login must never mean storing online-banking passwords, PINs, card security codes or authentication secrets in the application.

---

## 8. P1-01 decision result

**P1-01 scope decision:** Approved

The MVP, deferred-feature and excluded-feature boundaries are sufficiently defined for the project to continue.

Navigation, schema and migration-baseline approvals remain separate Phase 1 decisions and are not implied by this document.

---

## 9. Approval record

**Approved by:** Charlie Cook  
**Approval date:** 29 July 2026

**Notes:**

- Include every feature currently classified as Essential MVP.
- Keep useful-later features out of the first version while preserving reasonable future extensibility.
- Exclude the former optional features, except for moving the calendar task view into useful-later scope.
- Move bank reconciliation, Open Banking connections, automatic banking login and automatic tax return submission into useful-later scope.
- Ignore the remaining Section 6.4 features.
- Approve Section 32 only where it remains consistent with these decisions.
