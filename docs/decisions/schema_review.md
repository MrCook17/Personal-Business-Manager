# Schema Review

**Project:** Personal Business Manager  
**Decision:** P1-03 — Complete and record the schema review  
**Original review date:** 29 July 2026  
**Updated review date:** 29 July 2026  
**Owner and reviewer:** Charlie Cook  
**Technical review:** ChatGPT  
**Implementation and live verification:** Codex  
**Reviewed schema source:** `docs/personal_business_management_application_schema.sql`  
**Live amendment script:** `docs/schema_review_live_amendments.sql`  
**Primary source of truth:** `personal_business_management_application_final_plan.md`  
**Related approved decisions:** `phase_1_approval.md`, `workflow_codes.md`  
**Review status:** Approved  
**Phase 1 schema baseline approved:** Yes  
**Repository evidence status:** Pending Git tracking of the ignored `docs/` files

---

## 1. Executive decision

The 31-table Personal Business Manager schema is approved as the Phase 1 baseline.

The amendments identified during the original P1-03 review have now been implemented in:

- the live MariaDB development database;
- the empty-database bootstrap SQL;
- a standalone, tested one-time live amendment script.

The amended live database and a freshly bootstrapped disposable database were compared and reported as structurally identical:

```text
Metadata records in live database:       756
Metadata records in fresh bootstrap:     756
Differences:                               0
```

The schema now contains:

```text
Tables:                                   31
Enforced CHECK constraints:              116
Obsolete reviewed columns remaining:       0
```

The schema design, implemented constraints, data types, relationships, indexes, workflow-code enforcement and bootstrap behaviour are therefore approved.

One database limitation remains intentionally handled by the application layer: MariaDB 10.4 does not permit the desired self-reference check comparing `invoices.credit_for_invoice_id` with the table’s `AUTO_INCREMENT record_id`. The foreign key and credit-note structural checks remain in MariaDB, while prevention of self-credit must be enforced in the invoice application service and integration tests.

The absence of FluentMigrator is not a P1-03 schema-design failure. Migration baseline policy and FluentMigrator implementation belong to P1-07 and Phase 2.

### Final technical decision

```text
Schema design direction:                 APPROVED
Amended bootstrap schema:                APPROVED
Live development schema:                 APPROVED
Live/bootstrap structural parity:        VERIFIED
Destructive schema work still pending:   NO
P1-03 technical gate:                    PASS
```

### Repository condition

The repository currently ignores the entire `docs/` directory. As a result, the updated SQL files and this approval document will not appear in normal Git status unless that ignore policy is changed or the files are deliberately force-added.

P1-03 is technically complete, but its repository evidence requirement is not fully satisfied until the approved schema files and this review are committed.

---

## 2. Scope of the completed review

The completed review covers:

- all planned tables and supporting tables;
- lowercase `snake_case` naming;
- `record_id` primary keys;
- MariaDB data types;
- InnoDB and `utf8mb4`;
- defaults;
- unique constraints;
- foreign keys;
- delete behaviour;
- indexes;
- closed workflow-code checks;
- Boolean checks;
- archive-state consistency;
- completed-state consistency;
- invoice and credit-note structural rules;
- invoice-line and discount consistency;
- payment reversal consistency;
- financial precision;
- exact time-duration storage;
- seed-data behaviour;
- live-database amendment safety;
- empty-database bootstrap execution;
- restored-backup amendment testing;
- live-versus-bootstrap schema comparison;
- existing-data impact;
- compatibility with P1-01 and P1-02.

This updated decision is based on Codex’s reported execution and verification results. The SQL source files, amendment script, backup hash and command output should be retained in the repository or project evidence so the result remains independently reproducible.

---

## 3. Final schema evidence

| Item | Verified result |
|---|---:|
| Tables | 31 |
| Storage engine | InnoDB |
| Character set | `utf8mb4` |
| Tables using `record_id` primary keys | 31 of 31 |
| Enforced check constraints | 116 |
| Live metadata records | 756 |
| Fresh-bootstrap metadata records | 756 |
| Live/bootstrap metadata differences | 0 |
| Core test results | 1 passed |
| Integration test results | 1 passed |
| Test failures | 0 |
| Obsolete reviewed columns present | 0 |
| Financial values changed | No |
| Ambiguous existing rows requiring conversion | 0 |

The live development database was reported as MariaDB 10.4.32.

---

# 4. Approved schema decisions

## 4.1 Table set

The 31-table structure is approved.

It provides the planned foundations for:

- users and password recovery;
- application settings;
- schema information;
- audit history;
- backup history;
- customers, contacts and addresses;
- jobs, timers, time entries and tasks;
- personal and business financial accounts;
- account balance snapshots;
- financial account applications and contributions;
- invoices, credit notes, invoice lines, time links and payments;
- expenses and categories;
- attachments and entity-specific attachment links.

No Open Banking, automatic banking login, bank reconciliation, automatic tax submission or other deferred banking automation has been added.

## 4.2 Naming

Approved:

- lowercase `snake_case` table names;
- lowercase `snake_case` columns;
- `record_id` primary keys;
- `pk_`, `fk_`, `uq_`, `idx_` and `chk_` constraint naming.

The singleton `schema_information.record_id` type remains an accepted deliberate exception to the normal `BIGINT UNSIGNED AUTO_INCREMENT` pattern.

## 4.3 Storage engine and character set

All tables use InnoDB and `utf8mb4`.

The current collation remains an acceptable compatibility choice for the existing MariaDB 10.4.32 development server.

## 4.4 Workflow-code storage

Persisted workflow codes remain `VARCHAR` values rather than MariaDB `ENUM`.

This is approved because it supports:

- readable persisted values;
- shared C# string constants;
- deliberate migration-based changes;
- easier Dapper mapping;
- avoidance of numeric enum persistence mistakes.

Closed code sets are now also protected by MariaDB checks.

## 4.5 Money, quantities and rates

Approved:

```text
DECIMAL(18,2)  stored monetary totals and balances
DECIMAL(18,4)  quantities and unit rates
DECIMAL(7,4)   percentages and rates
```

No floating-point SQL type is used for financial values.

## 4.6 Dates and times

Approved:

- `DATETIME(6)` for UTC timestamps;
- `DATE` for date-only concepts;
- application-controlled timestamp creation;
- UTC-oriented database sessions;
- exact raw and rounded duration storage in seconds.

## 4.7 Critical uniqueness

The schema protects:

- normalised usernames;
- one active timer per user;
- job numbers;
- invoice numbers;
- invoice line ordering within an invoice;
- one invoice link per time entry;
- financial account type codes;
- one opened account link per account application;
- invoice-number sequence code/year combinations;
- duplicate attachment links.

## 4.8 Foreign-key deletion policy

Approved:

- important business and financial relationships use `ON DELETE RESTRICT`;
- selected attribution relationships may use `SET NULL`;
- attachment link rows use `ON DELETE CASCADE`;
- password recovery codes use `ON DELETE CASCADE`.

Normal application workflows must still archive important records rather than physically deleting them.

## 4.9 Attachment-link cascades

The attachment link tables intentionally cascade only removal of the link rows.

This is approved and does not imply cascade deletion of unrelated principal records.

## 4.10 `invoice_payments` exception

`invoice_payments` intentionally has no `date_updated_utc`.

This remains approved because reversals retain:

- reversal state;
- reversal timestamp;
- nonblank reversal reason;
- optimistic-concurrency version;
- audit history.

Payments are reversed, not edited or deleted.

## 4.11 Bootstrap role

`personal_business_management_application_schema.sql` is approved as the empty-database bootstrap specification.

It must not be used as an upgrade mechanism for an existing database.

The tested `schema_review_live_amendments.sql` is the one-time amendment record for bringing the existing development schema into line before FluentMigrator baseline work.

After migrations are introduced:

- empty databases must be reproducible through FluentMigrator;
- existing databases must be registered safely at the approved baseline;
- released migrations must never be edited;
- future schema changes must use new migrations.

---

# 5. Amendment completion record

## SR-001 — Exact rounded duration storage

**Status:** Complete

Replaced:

```text
time_entries.rounded_duration_minutes
```

with:

```text
time_entries.rounded_duration_seconds BIGINT UNSIGNED
```

This correctly supports:

```text
none
nearest_5
nearest_6
nearest_10
nearest_15
up_5
up_6
up_10
up_15
```

For `none`, rounded seconds can exactly equal raw seconds.

No existing time rows required conversion:

```text
time_entries rows before change: 0
```

## SR-002 — Remove duplicated billed minutes

**Status:** Complete

Removed:

```text
invoice_time_entries.billed_minutes
```

Retained:

```text
invoice_time_entries.billed_seconds
```

Invoice display quantities are derived from billed seconds while billed rate and amount remain the immutable financial snapshot.

No existing invoice-time rows required conversion:

```text
invoice_time_entries rows before change: 0
```

## SR-003 — Remove deferred recurrence placeholder

**Status:** Complete

Removed:

```text
tasks.recurrence_definition_id
```

No task row contained recurrence data.

Recurring tasks remain deferred and will require a future migration when the feature is approved.

The current task fields remain sufficient for a later calendar task view.

## SR-004 — Closed-code constraints

**Status:** Complete

Closed-code constraints were added for the requested controlled vocabularies covering:

- users;
- application settings;
- backups;
- customers;
- customer addresses;
- jobs;
- time entries;
- tasks;
- financial accounts;
- balance snapshots;
- account applications;
- contributions;
- invoice sequences;
- invoices;
- invoice lines;
- payments;
- expenses.

Standards-based and extensible values remain outside rigid closed checks where appropriate, including ISO country/currency values, data-driven account types and extensible audit codes.

## SR-005 — System audit actors

**Status:** Complete

Changed:

```text
audit_records.user_id
```

to nullable.

Interpretation:

- non-null: resolved application user;
- null: system action or unresolved-user event.

This supports backup, startup, consistency and failed-login events without inventing a fake user.

## SR-006 — Required expense payment method

**Status:** Complete

Changed:

```text
expenses.payment_method_code
```

to required.

No existing expense row had a null payment method:

```text
affected rows: 0
```

## SR-007 — Lifecycle consistency

**Status:** Complete

Added or confirmed checks for:

- customer active/archive consistency;
- job completed status and timestamp consistency;
- task completed status and timestamp consistency;
- financial-account closed status and date consistency;
- invoice draft/cancelled versus finalised structural state;
- credit-note reference consistency.

## SR-008 — Invoice-line and discount consistency

**Status:** Complete

Added or confirmed checks for:

- `none` discounts with zero discount values;
- percentage discount limits;
- fixed discount nonnegative requirements;
- credit lines requiring an original invoice-line reference;
- non-credit lines rejecting a credit reference;
- invoice and expense total consistency;
- valid positive invoice line numbers.

## SR-009 — Boolean checks

**Status:** Complete

Explicit checks now reject values outside `0` and `1` for the requested Boolean columns.

This avoids relying on `TINYINT(1)` as though it alone were a Boolean domain.

## SR-010 — Preserve invoice prefixes

**Status:** Complete

Bootstrap reseeding no longer overwrites an existing configured invoice-number prefix.

Verification used a custom prefix:

```text
CUSTOM-
```

After rerunning the bootstrap seed logic, the custom value remained unchanged.

Existing sequence numbers must also never be reduced.

## SR-011 — Supporting checks

**Status:** Complete, with one accepted application-service rule

Confirmed or added:

- positive rounded duration;
- archive-date ordering;
- password-recovery timestamp ordering;
- invoice-line number validation;
- invoice and expense total equations;
- credit-note and credit-line structural consistency;
- nonblank payment reversal reason;
- relevant account/application consistency;
- existing reviewed financial and timestamp checks.

### Accepted MariaDB limitation

MariaDB 10.4 rejected a `CHECK` directly comparing:

```text
credit_for_invoice_id
```

with the same row’s:

```text
AUTO_INCREMENT record_id
```

The schema still enforces:

- the self-referencing foreign key;
- credit-note type/reference consistency;
- non-credit-note null-reference consistency.

The invoice application service must additionally enforce:

```text
credit_for_invoice_id != record_id
```

This must be covered by an integration test when invoice workflows are implemented.

This limitation is accepted and does not block the schema baseline.

---

# 6. Existing-data handling and safety

Before changing the live database, Codex reported:

```text
time_entries rows:                         0
invoice_time_entries rows:                 0
tasks containing recurrence data:          0
expenses with null payment method:         0
invoices rows:                             0
expenses rows:                             0
financial_accounts rows:                   0
users rows:                                0
```

Therefore:

- no ambiguous duration conversion was required;
- no billed-duration data was lost;
- no recurrence data was discarded;
- no manual payment-method decision was required;
- no existing invoice or expense amount changed;
- no user or account data required repair.

Reported post-change totals remained:

```text
Invoice totals: 0.00
Expense totals: 0.00
```

No financial value changed.

---

# 7. Backup and disposable-database verification

A pre-change backup was created at:

```text
C:\Users\Charl\AppData\Local\Temp\personal_business_manager_pre_schema_review_20260729_180824.sql
```

Reported size:

```text
64,231 bytes
```

The backup was SHA-256 hashed after creation.

The reported verification procedure was:

1. create and hash the pre-change backup;
2. restore the backup into a disposable database;
3. run the exact amendment script against the restored copy;
4. confirm successful amendment execution;
5. run the amended empty-database bootstrap independently;
6. compare the amended live schema with the fresh bootstrap;
7. confirm row counts and financial totals;
8. run the solution tests.

The temporary backup path is not a durable repository evidence location. Retain the hash and any required backup evidence in a secure project record before the operating system clears the temporary directory.

---

# 8. Live and bootstrap verification

## 8.1 Restored-copy amendment test

The exact live amendment script was successfully executed against a disposable database restored from the pre-change backup.

This proves the amendment procedure was not tested first against only the live development database.

## 8.2 Empty bootstrap test

The amended bootstrap was independently executed on MariaDB 10.4.32.

Reported result:

```text
Tables created:              31
Enforced check constraints: 116
```

## 8.3 Structural comparison

Live database and freshly bootstrapped database:

```text
Metadata records: 756 each
Differences:      0
```

The comparison covered columns, constraints and indexes.

All obsolete columns were reported absent.

## 8.4 Tests

Command:

```powershell
dotnet test PersonalBusinessManager.slnx --no-restore
```

Reported result:

```text
Core tests:         1 passed
Integration tests:  1 passed
Failures:           0
```

The current tests remain minimal and do not prove the later application workflows. Expanding meaningful unit and integration coverage remains a Phase 2 requirement.

Existing logging analyser warnings remain and belong to P2-14.

---

# 9. Application-service invariants

The following rules remain intentionally outside simple row-level checks and must be implemented in services and transactions.

## 9.1 Customers

- one active primary contact per customer;
- one active default address per customer/address type;
- archived customers cannot normally receive new jobs;
- customer edits do not change finalised invoice snapshots.

## 9.2 Jobs and time

- completed/cancelled jobs reject timers until reopened;
- one active timer per user;
- stopping a timer creates the entry and removes the timer atomically;
- raw duration agrees with timestamps;
- corrections require a reason and audit record;
- finalised-invoice-linked time cannot be edited directly.

## 9.3 Accounts

- invoice-payment and expense accounts must be business-scope accounts;
- account classification controls asset/liability treatment;
- tax-wrapper type must agree with account type;
- application conversion creates and links the account atomically;
- balance snapshot creation and current-balance update are atomic;
- contributions do not automatically alter balances.

## 9.4 Invoices

- source jobs belong to the invoice customer;
- selected time belongs to the correct customer/job;
- invoice-time links belong only to time lines;
- credit lines reference lines from the credited invoice;
- a credit note cannot reference itself;
- credits cannot exceed available uncredited value without an approved override;
- finalised financial records are immutable;
- numbering locks the sequence row;
- PDF generation uses committed invoice data;
- payment/credit changes recalculate status and outstanding amount atomically.

---

# 10. Accepted exceptions

The following remain accepted:

- singleton `schema_information` key type;
- no `date_updated_utc` on `invoice_payments`;
- attachment-link `ON DELETE CASCADE`;
- password-recovery cascade;
- nullable actors for valid system-originated events;
- data-driven financial-account types;
- additional backup and schema-information tables;
- no seeded administrator;
- self-credit prevention enforced by the application service because of the MariaDB 10.4 check limitation.

---

# 11. Index review

The existing index plan is approved for the baseline.

It supports the planned access patterns for:

- customer identity and archive state;
- contacts and addresses;
- jobs by customer, status, priority and due date;
- time by user, job, billable state and date;
- tasks by job, status, priority and due date;
- accounts by scope, type, status, maturity and provider;
- snapshots by account/date;
- applications by workflow and next-action date;
- invoices by customer/date/status/due date;
- payments by invoice and receiving account;
- expenses by date/category/account/supplier;
- audit records by entity/user/correlation;
- attachment hashes and reverse links.

Future indexes must be justified using representative queries and execution plans rather than added speculatively.

---

# 12. Seed-data review

Approved baseline seed data includes:

- financial account type lookup rows;
- `Uncategorised` expense category;
- separate invoice and credit-note sequences;
- initial non-sensitive application settings;
- no user/password seed;
- GBP, GB, `en-GB` and dark-theme defaults;
- a conservative zero tax-reserve setting.

Seed values are initial configuration, not permanent business logic.

The bootstrap now preserves an existing configured invoice prefix.

---

# 13. FluentMigrator and baseline implications

There is currently no FluentMigrator project in the repository.

This does not invalidate the approved schema. It means:

- the tested standalone amendment script is the current one-time development amendment record;
- P1-07 must approve the exact migration baseline strategy;
- Phase 2 must implement the initial FluentMigrator migrations;
- empty databases must then be reproducible through migrations;
- the existing development database must be registered at the baseline without replaying table creation;
- the amendment script must not become a substitute for future versioned migrations.

No automatic migration or baseline action should run during normal WinForms startup.

---

# 14. Repository tracking issue

The repository’s `.gitignore` currently ignores the entire:

```text
docs/
```

directory.

Consequences:

- the updated bootstrap SQL may not be tracked;
- the amendment script may not be tracked;
- this schema review may not be tracked;
- normal `git status` may falsely appear clean.

## Required resolution

Prefer changing `.gitignore` so project documentation and schema sources are tracked normally.

A suitable policy is to stop ignoring `docs/`, or add deliberate exceptions for the required files/directories.

After correcting the rule, verify:

```powershell
git status
git check-ignore -v docs/personal_business_management_application_schema.sql
git check-ignore -v docs/schema_review_live_amendments.sql
git check-ignore -v docs/decisions/schema_review.md
```

Then stage and commit the approved evidence.

Force-adding with `git add -f` is acceptable only as a deliberate temporary measure; correcting the overly broad ignore rule is preferable.

---

# 15. Final verification checklist

## Structural review

- [x] The 31-table schema is approved.
- [x] Lowercase `snake_case` naming is approved.
- [x] All tables use `record_id` primary keys.
- [x] `VARCHAR` workflow codes are intentional.
- [x] `invoice_payments` intentionally has no `date_updated_utc`.
- [x] Attachment link tables intentionally use `ON DELETE CASCADE`.
- [x] Core business and finance relationships use `ON DELETE RESTRICT`.
- [x] Critical unique constraints and foreign keys are present.
- [x] The amended SQL is approved as the empty-database bootstrap specification.

## Original amendments

- [x] Replace rounded minutes with exact rounded seconds.
- [x] Remove duplicated billed minutes.
- [x] Remove the deferred recurrence placeholder.
- [x] Add approved closed-code checks.
- [x] Make `audit_records.user_id` nullable.
- [x] Make expense payment method required.
- [x] Add lifecycle consistency checks.
- [x] Add invoice-line and discount consistency checks.
- [x] Add explicit Boolean checks.
- [x] Preserve configured invoice prefixes.
- [x] Add or confirm supporting checks.

## Existing-data safety

- [x] Existing affected rows were inspected.
- [x] No ambiguous duration conversion was required.
- [x] No recurrence data was removed.
- [x] No expense payment method required manual repair.
- [x] No financial values changed.
- [x] A pre-change backup was created and hashed.

## Runtime verification

- [x] Backup restored to a disposable database.
- [x] Exact live amendment script tested against restored copy.
- [x] Empty bootstrap tested independently.
- [x] 31 tables confirmed.
- [x] 116 enforced checks confirmed.
- [x] Live and bootstrap columns compared.
- [x] Live and bootstrap constraints compared.
- [x] Live and bootstrap indexes compared.
- [x] 756 metadata records matched on both sides.
- [x] Zero metadata differences found.
- [x] Obsolete columns confirmed absent.
- [x] Custom invoice prefix preservation tested.
- [x] Existing tests passed.

## Remaining project actions outside the technical schema approval

- [!] Correct `.gitignore` and commit the `docs/` schema evidence.
- [ ] Approve the P1-07 migration baseline strategy.
- [ ] Implement FluentMigrator in Phase 2.
- [ ] Add the invoice-service self-credit validation and integration test.
- [ ] Expand meaningful unit and integration tests under P2-08.
- [ ] Resolve or document existing analyser warnings under P2-14.

---

# 16. Final P1-03 decision

```text
Schema design direction:                     APPROVED
Current amended SQL as final baseline:        APPROVED
Live development database schema:             APPROVED
Live/bootstrap parity:                        VERIFIED
Required destructive schema changes pending: NO
P1-03 technical gate:                        PASS
Repository evidence committed:               NO — docs/ currently ignored
```

The schema is now technically complete and approved as the Phase 1 baseline.

To satisfy the repository/documentation evidence requirement fully, update the `.gitignore` policy and commit:

```text
docs/personal_business_management_application_schema.sql
docs/schema_review_live_amendments.sql
docs/decisions/schema_review.md
```

Once those files are tracked and committed, P1-03 can be marked fully complete without conditions.

---

## 17. Approval record

**Owner:** Charlie Cook  
**Original review:** ChatGPT  
**Implementation and verification:** Codex  
**Approval date:** 29 July 2026  
**Current technical approval:** Approved  
**Repository completion:** Pending Git tracking

**Accepted exception requiring later application enforcement:**

```text
A credit note must not reference itself.
```

MariaDB 10.4 could not enforce this through the attempted row-level check involving the auto-increment key. The invoice application service and integration tests must enforce it before invoice functionality is considered complete.
