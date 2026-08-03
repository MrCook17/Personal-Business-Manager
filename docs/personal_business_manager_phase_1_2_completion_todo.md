# Personal Business Manager — Phase 1 and Phase 2 Completion Todo List

**Project:** Personal Business Manager  
**Target:** Complete Phase 1 and Phase 2 before beginning Phase 3  
**Source of truth:** `personal_business_management_application_final_plan.md`  
**Current audit result:** Phase 1 incomplete; Phase 2 incomplete  
**Last updated:** 28 July 2026

---

## How to use this checklist

Complete the tasks in the recommended order below.

Status markers:

- `[ ]` Not started
- `[~]` In progress
- `[x]` Complete
- `[!]` Blocked or requires a decision

A task is only complete when its **evidence required** section exists and its **verification** steps pass.

---

# Recommended execution order

1. Record the Phase 1 owner decisions and approvals.
2. Finalise workflow/code values.
3. Create the missing wireframes.
4. Create worked financial calculation examples.
5. Decide the FluentMigrator baseline strategy.
6. Replace the MariaDB `root` runtime account.
7. Add FluentMigrator safely.
8. Add a real Dapper repository read/write test.
9. Add meaningful unit and integration tests.
10. Complete the minimum Phase 2 theme and shell infrastructure.
11. Add the list/search/paging foundation.
12. Resolve or document build warnings.
13. Complete the final Phase 1 and Phase 2 gate review.

---

# Phase 1 — Requirements, wireframes and schema

## P1-01 — Formally freeze and approve the MVP scope

**Priority:** Critical  
**Blocks Phase 3:** Yes, as an owner decision  
**Estimated size:** Very small

- [ ] Read Sections 6 and 32 of the final plan.
- [ ] Confirm the features listed as **Essential MVP** are correct.
- [ ] Confirm the features listed as **Useful later** are deferred.
- [ ] Confirm the features listed as **Optional** remain optional.
- [ ] Confirm the features listed as **Avoid in the first version** are excluded.
- [ ] Record the approval in a repository document.

**Create:**

```text
docs/decisions/phase_1_approval.md
```

**Minimum content:**

```markdown
# Phase 1 Approval

Date:
Owner: Charlie Cook

- MVP scope approved: Yes
- Deferred features approved: Yes
- Optional features approved: Yes
- Excluded first-version features approved: Yes
- Navigation approved: Yes/No
- Schema approved: Yes/No
- Migration order approved: Yes/No

Notes:
```

**Evidence required:**

- A committed approval document.
- No unresolved scope notes that affect Phase 2 or Phase 3.

**Verification:**

- [ ] The approval document is committed to Git.
- [ ] The plan and approval document do not conflict.

---

## P1-02 — Finalise all workflow and code values

**Priority:** Critical  
**Blocks Phase 3:** Yes  
**Estimated size:** Small

Create a single reference document containing every allowed code value.

**Create:**

```text
docs/reference/workflow_codes.md
```

**Confirm and document:**

- [ ] Customer active/archive behaviour.
- [ ] Job statuses:
  - `planned`
  - `active`
  - `on_hold`
  - `completed`
  - `cancelled`
- [ ] Job priorities:
  - `low`
  - `normal`
  - `high`
  - `urgent`
- [ ] Charging types:
  - `hourly`
  - `fixed_price`
  - `mixed`
  - `non_billable`
- [ ] Task statuses:
  - `not_started`
  - `in_progress`
  - `blocked`
  - `completed`
  - `cancelled`
- [ ] Invoice types:
  - `invoice`
  - `credit_note`
- [ ] Invoice statuses:
  - `draft`
  - `finalised`
  - `sent`
  - `part_paid`
  - `paid`
  - `cancelled`
  - `credited`
- [ ] Invoice line types:
  - `time`
  - `fixed_price`
  - `manual`
  - `expense_recharge`
  - `adjustment`
  - `credit`
- [ ] Financial account classifications:
  - `asset`
  - `liability`
- [ ] Account scopes:
  - `business`
  - `personal`
- [ ] Financial account statuses — explicitly decide the allowed values.
- [ ] Time-entry methods — explicitly decide all allowed values, not only `manual`.
- [ ] Time-rounding rules:
  - `none`
  - `nearest_5`
  - `nearest_6`
  - `nearest_10`
  - `nearest_15`
  - `up_5`
  - `up_6`
  - `up_10`
  - `up_15`
- [ ] Account-application statuses:
  - `considering`
  - `planned`
  - `applied`
  - `identity_check`
  - `awaiting_information`
  - `approved`
  - `declined`
  - `withdrawn`
  - `opened`
  - `completed`

**Decisions required:**

- [ ] Decide whether invalid workflow codes are prevented only in C# or also through MariaDB check constraints.
- [ ] Decide whether workflow codes will use:
  - C# static constants;
  - enums with explicit persistence mapping; or
  - strongly typed value objects.
- [ ] Confirm whether existing database defaults use only approved codes.

**Evidence required:**

- One committed code-value document.
- A recorded decision on database constraints versus application validation.
- No undefined status fields in the current schema.

**Verification:**

- [ ] Search the SQL schema for every `*_code` and `status_code` column.
- [ ] Every code column is represented in `workflow_codes.md`.
- [ ] Existing defaults are valid.
- [ ] No conflicting spellings exist.

---

## P1-03 — Complete and record the schema review

**Priority:** High  
**Blocks Phase 3:** Yes, as an owner decision  
**Estimated size:** Small

The schema implementation passed the audit, but formal review decisions are still required.

- [ ] Confirm the 31-table schema is approved.
- [ ] Confirm lowercase `snake_case` naming is approved.
- [ ] Confirm all primary keys use `record_id`.
- [ ] Confirm MariaDB `VARCHAR` workflow codes are intentional.
- [ ] Confirm `invoice_payments` intentionally has no `date_updated_utc`.
- [ ] Confirm attachment link tables intentionally use `ON DELETE CASCADE`.
- [ ] Confirm core business and finance relationships use `ON DELETE RESTRICT`.
- [ ] Confirm all required unique constraints and foreign keys are present.
- [ ] Confirm the existing schema script is the bootstrap source for empty databases.

**Create or update:**

```text
docs/decisions/schema_review.md
```

**Evidence required:**

- Review date.
- Reviewer/owner.
- Accepted exceptions.
- Any required follow-up migration.
- Explicit statement that the schema is approved as the Phase 1 baseline.

**Verification:**

- [ ] The checked-in SQL script matches the live development schema.
- [ ] Any accepted differences are documented.
- [ ] No destructive schema changes are pending.

---

## P1-04 — Create low-fidelity wireframes

**Priority:** Critical  
**Blocks Phase 3:** Partly; login and settings wireframes should exist before Phase 3  
**Estimated size:** Medium

Create wireframes as Markdown diagrams, image mock-ups, or design files. They do not need polished visual design.

**Create folder:**

```text
docs/wireframes/
```

**Required wireframes:**

- [ ] Login.
- [ ] Main shell.
- [ ] Dashboard.
- [ ] Customer list.
- [ ] Customer details.
- [ ] Job list.
- [ ] Job details.
- [ ] Time list and active timer.
- [ ] Task list.
- [ ] Invoice list.
- [ ] Invoice editor/viewer.
- [ ] Business finance.
- [ ] Personal account list.
- [ ] Personal account details.
- [ ] Account applications.
- [ ] Audit history.
- [ ] Backups.
- [ ] Settings.

**Each data-heavy screen should show:**

- [ ] Normal state.
- [ ] Empty state.
- [ ] Loading state.
- [ ] Error/retry state.
- [ ] Validation state.
- [ ] Main actions.
- [ ] Search/filter position.
- [ ] Paging position.
- [ ] Detail navigation.
- [ ] Archive visibility where relevant.

**Suggested file structure:**

```text
docs/wireframes/
├── 01_login.md
├── 02_main_shell.md
├── 03_dashboard.md
├── 04_customers.md
├── 05_customer_details.md
├── 06_jobs.md
├── 07_job_details.md
├── 08_time.md
├── 09_tasks.md
├── 10_invoices.md
├── 11_invoice_editor.md
├── 12_business_finance.md
├── 13_personal_accounts.md
├── 14_account_details.md
├── 15_applications.md
├── 16_audit_history.md
├── 17_backups.md
└── 18_settings.md
```

**Evidence required:**

- All listed wireframes exist.
- The navigation hierarchy matches the plan.
- Owner approval is recorded.

**Verification:**

- [ ] Every permanent sidebar destination has a wireframe.
- [ ] Every main detail page has a wireframe.
- [ ] Phase 3 login/settings work can be implemented without inventing layout decisions.

---

## P1-05 — Complete the dark-theme design specification

**Priority:** High  
**Blocks Phase 3:** No, but should be completed before more screens are built  
**Estimated size:** Small

The colour palette exists, but typography and spacing are still scattered.

**Create or update:**

```text
docs/design/dark_theme_system.md
```

**Document:**

- [ ] Colour tokens.
- [ ] Typography sizes and weights.
- [ ] Spacing tokens based on 4/8/16/24/32 pixels.
- [ ] Standard control heights.
- [ ] Standard page padding.
- [ ] Grid row heights.
- [ ] Border widths.
- [ ] Focus styling.
- [ ] Disabled styling.
- [ ] Hover styling.
- [ ] Error, warning and success styling.
- [ ] DPI scaling expectations.
- [ ] Minimum supported window size.

**Evidence required:**

- Shared design document.
- Matching C# theme infrastructure in Phase 2.

**Verification:**

- [ ] No new screen needs to invent its own spacing or font sizes.
- [ ] Theme tokens match the plan.

---

## P1-06 — Create agreed worked financial calculation examples

**Priority:** Critical  
**Blocks Phase 3:** No, but blocks later finance implementation and full Phase 1 sign-off  
**Estimated size:** Medium

Create concrete input/output examples rather than formulas only.

**Create:**

```text
docs/reference/financial_calculation_examples.md
```

**Required examples:**

### Invoice calculations

- [ ] Quantity × unit rate.
- [ ] Percentage line discount.
- [ ] Fixed-amount line discount.
- [ ] VAT after discount.
- [ ] Two-decimal monetary rounding.
- [ ] `MidpointRounding.AwayFromZero`.
- [ ] Multiple lines with stored rounded totals.
- [ ] VAT-inclusive pricing.
- [ ] VAT-exclusive pricing.

### Payments and credit notes

- [ ] No payment.
- [ ] Part payment.
- [ ] Fully paid.
- [ ] Overpayment requiring confirmation.
- [ ] Partial credit note.
- [ ] Full credit note.
- [ ] Payment reversal.

### Time calculations

- [ ] Raw duration.
- [ ] Each rounding rule.
- [ ] Manual entry.
- [ ] Billable versus non-billable.
- [ ] Billed duration and rate snapshot.

### Reporting

- [ ] Invoiced revenue.
- [ ] Received income.
- [ ] Invoiced profit estimate.
- [ ] Cash profit estimate.
- [ ] Tax-reserve estimate.
- [ ] VAT estimate.

### Personal finance

- [ ] Positive asset.
- [ ] Negative current-account asset balance.
- [ ] Positive liability.
- [ ] Negative liability/credit balance.
- [ ] Net-worth total.
- [ ] Contributions not automatically changing account balance.

**Evidence required:**

- Each example includes exact inputs, calculation steps and expected result.
- Owner approval is recorded.
- Matching unit tests are added during Phase 2 completion.

**Verification:**

- [ ] A developer can implement the calculations without guessing.
- [ ] Edge cases are explicitly covered.

---

## P1-07 — Approve the migration order and baseline policy

**Priority:** Critical  
**Blocks Phase 2 completion:** Yes  
**Estimated size:** Small

The migration order exists, but the already-created schema needs a formal baseline policy.

**Create:**

```text
docs/decisions/migration_baseline_strategy.md
```

**The decision must state:**

- [ ] The current manually created schema is the initial baseline.
- [ ] Existing databases must not replay table-creation migrations.
- [ ] Empty future databases must be buildable through migrations.
- [ ] The migration history/version table will be introduced safely.
- [ ] A backup is required before baseline registration.
- [ ] Baseline work is tested against a disposable database copy first.
- [ ] Runtime and migration database accounts are separate where practical.
- [ ] Released migrations are never edited.
- [ ] Schema changes after baseline always use new migrations.

**Choose and document one safe approach:**

### Recommended approach

- [ ] Implement the planned initial schema as migrations for creating a new empty database.
- [ ] Verify those migrations create a schema equivalent to the existing bootstrap SQL.
- [ ] Add a controlled one-time baseline command/tool that records those initial versions as already applied on the existing database without executing their `Up` methods.
- [ ] Require an explicit confirmation and backup before baselining.
- [ ] Do not automatically baseline during normal application startup.

**Evidence required:**

- Approved strategy document.
- Exact baseline version identified.
- Rollback/recovery steps documented.

**Verification:**

- [x] Empty test database can be created through migrations.
- [ ] Copy of existing database can be baselined without table recreation.
- [ ] Both schemas compare successfully.

---

# Phase 2 — Solution shell and MariaDB foundation

## P2-01 — Replace the MariaDB `root` runtime account

**Priority:** Critical  
**Blocks Phase 3:** Yes  
**Estimated size:** Small

The application currently connects as `root@localhost` with global privileges.

- [ ] Change the password that was previously exposed in chat or terminal history.
- [ ] Create a dedicated runtime account such as:

```text
personal_business_app@localhost
```

- [ ] Restrict it to the application database.
- [ ] Do not grant global privileges.
- [ ] Do not grant `GRANT OPTION`.
- [ ] Do not use it for arbitrary server administration.
- [ ] Set `PBM_CONNECTION_STRING` to the dedicated account.
- [ ] Remove any process-level value that still points to `root`.
- [ ] Restart the application and verify the runtime identity.

**Privilege design:**

The runtime account should have only the operations required by the application. During early development this may include:

```text
SELECT
INSERT
UPDATE
DELETE
```

on:

```text
personal_business_manager.*
```

Schema-altering privileges should belong to a separate migration/admin account.

**Verification queries:**

```sql
SELECT CURRENT_USER();
SHOW GRANTS FOR CURRENT_USER();
```

**Expected result:**

- [ ] Current user is the dedicated application account.
- [ ] No `ALL PRIVILEGES ON *.*`.
- [ ] No `WITH GRANT OPTION`.
- [ ] Access is restricted to the application database and local host.

**Evidence required:**

- Redacted output showing account and grants.
- Application log showing successful connection.
- No credential committed to Git.

---

## P2-02 — Record the MariaDB development-version decision

**Priority:** Medium  
**Blocks Phase 3:** No, provided the current server remains development-only  
**Estimated size:** Very small

The current XAMPP server is MariaDB 10.4.32. The plan prefers MariaDB 11.8 LTS for the maintained production baseline.

**Create:**

```text
docs/decisions/mariadb_version_strategy.md
```

**Decide and document:**

- [ ] MariaDB 10.4.32 is temporary local development infrastructure only.
- [ ] The application will be validated against the approved maintained LTS version before production use.
- [ ] The upgrade/migration will be tested with backups and restore.
- [ ] No production deployment will depend indefinitely on the old XAMPP server.
- [ ] The minimum currently tested server version is recorded.

**Verification:**

- [ ] The decision is committed.
- [ ] No documentation incorrectly calls 10.4.32 the production baseline.

---

## P2-03 — Add FluentMigrator packages and runner infrastructure

**Priority:** Critical  
**Blocks Phase 3:** Yes for full Phase 2 completion  
**Estimated size:** Medium

Do this only after P1-07 is approved.

- [x] Add the FluentMigrator runner package to Infrastructure or a dedicated migration project/tool.
- [x] Add the MariaDB/MySQL FluentMigrator provider.
- [x] Add migration service registration.
- [x] Add a migration runner that is not silently executed against the existing database.
- [x] Add logging for migration start, success and failure without credentials.
- [x] Use a separate migration/admin connection where practical.
- [x] Add explicit command-line or development-only migration execution.
- [x] Prevent accidental migration execution from normal WinForms page code.

**Recommended structure:**

```text
src/PersonalBusinessManager.Infrastructure/Database/Migrations/
├── Migration_0001_CreateUsersAndSecurity.cs
├── Migration_0002_CreateSettingsAndAudit.cs
├── ...
└── Migration_0013_SeedInitialSettings.cs
```

A dedicated migration console/tool is also acceptable:

```text
tools/PersonalBusinessManager.DatabaseMigrator/
```

**Verification:**

- [x] Solution builds.
- [x] Migration runner can report pending migrations.
- [x] It requires an explicit connection and execution command.
- [x] It does not replay initial migrations against the existing live schema.

**Completion evidence — 30 July 2026:**

- FluentMigrator Runner Core and the MySQL/MariaDB provider are registered in Infrastructure.
- `schema_migrations` uses the approved lowercase `snake_case` metadata names.
- `tools/PersonalBusinessManager.DatabaseMigrator/` provides read-only `status` and guarded `migrate` commands.
- The connection must be selected explicitly through `PBM_MIGRATION_CONNECTION_STRING` or `PBM_TEST_MIGRATION_CONNECTION_STRING`; raw command-line connection strings and the runtime connection variable are rejected.
- `migrate` requires exact `MIGRATE <database_name>` confirmation.
- An existing schema with no migration history is blocked before FluentMigrator can execute, reserving it for the P2-05 `baseline-existing` workflow.
- Migration execution is never registered or invoked by WinForms startup or page code.
- Solution build and all tests pass; migration infrastructure tests cover registration, metadata, discovery, command parsing, and safety guards.

---

## P2-04 — Implement and test the initial migrations

**Priority:** Critical  
**Blocks Phase 3:** Yes for full Phase 2 completion  
**Estimated size:** Large

Implement the documented migration sequence:

- [x] `0001_create_users_and_security`
- [x] `0002_create_application_settings_and_audit`
- [x] `0003_create_customers_contacts_and_addresses`
- [x] `0004_create_jobs_tasks_and_time_tracking`
- [x] `0005_create_financial_account_types_and_accounts`
- [x] `0006_create_account_snapshots_applications_and_contributions`
- [x] `0007_create_invoice_sequences_invoices_and_lines`
- [x] `0008_create_invoice_time_links_and_payments`
- [x] `0009_create_expense_categories_and_expenses`
- [x] `0010_create_attachments_and_link_tables`
- [x] `0011_seed_core_lookup_data`
- [x] `0012_create_required_indexes_and_constraints`
- [x] `0013_seed_initial_application_settings`

**Rules:**

- [x] Every migration has an `Up`.
- [x] Add a safe `Down` only where practical.
- [x] Do not use destructive `Down` operations casually.
- [x] Do not modify a migration after it has been released.
- [x] Preserve lowercase `snake_case`.
- [x] Preserve `record_id`.
- [x] Preserve InnoDB and `utf8mb4`.
- [x] Preserve constraints and indexes.
- [x] Seed data is deterministic and idempotent where appropriate.

**Verification:**

- [x] Apply all migrations to an empty disposable MariaDB database.
- [x] Compare resulting schema to the approved schema.
- [x] Verify table, column, index, FK, unique and check counts.
- [x] Verify core seed data.
- [x] Verify migration version table exists.
- [x] Run the upgrade/baseline test against a copy of the existing database.
- [x] Never test destructive migration behaviour against the real development database first.

### P2-04 completion evidence - 30 July 2026

- Implemented FluentMigrator versions `1` through `13` in Infrastructure using
  the exact approved bootstrap definitions and the approved migration order.
- MariaDB DDL migrations use `TransactionBehavior.None` because MariaDB commits
  DDL implicitly.
- Destructive table and mutable-seed rollback is rejected explicitly. Migration
  `0012` has a tested definition-level rollback that drops its 59 secondary
  indexes in reverse order.
- Automated parity tests compare the migration definitions and seed statements
  with `docs/personal_business_management_application_schema.sql`.
- A fresh disposable MariaDB 10.4.32 database successfully applied all 13
  migrations. `schema_migrations` contained versions `1` through `13`,
  `schema_information.schema_version` was `13`, and no migrations remained.
- The migrated and freshly bootstrapped disposable schemas each produced 756
  normalized metadata records with zero differences: 31 application tables,
  373 columns, 128 named indexes, 31 primary keys, 18 unique constraints, 56
  foreign keys, and 116 checks.
- All 31 application tables used `record_id` primary keys, InnoDB, and
  `utf8mb4_unicode_ci`.
- Seeds verified as 17 financial-account types, one `Uncategorised` expense
  category, two invoice sequences, and 18 application settings. Replaying the
  approved idempotent seed logic preserved a customized invoice prefix and
  next number and a customized application-setting value.
- The current development database was streamed read-only into a uniquely named
  disposable copy. Attempting normal migration was correctly blocked as an
  existing unversioned schema; no history table was created and all 31
  per-table row counts remained identical to the source.
- The guarded `baseline-existing` registration itself remains P2-05 and was not
  simulated or applied here.
- The real `personal_business_manager` database was never altered. All three
  disposable verification databases were removed after the checks.
- Solution build and all tests pass. Migration tests cover exact schema and seed
  parity, object counts and conventions, migration discovery, history metadata,
  command parsing, safety guards, credential-statement exclusion, and the safe
  secondary-index rollback definition.

---

## P2-05 — Baseline the existing schema safely

**Priority:** Critical  
**Blocks Phase 3:** Yes for full Phase 2 completion  
**Estimated size:** Medium

- [x] Back up the current development database.
- [x] Restore it into a disposable test database.
- [x] Run the controlled baseline process against the disposable copy.
- [x] Confirm no `CREATE TABLE` migration is replayed.
- [x] Confirm the version table records the approved baseline.
- [x] Confirm no data or schema object changed unexpectedly.
- [x] Run all later pending migrations successfully.
- [x] Document the production/development baseline procedure.
- [x] Only then baseline the real development database.

**Evidence required:**

- Baseline log with no credentials.
- Schema comparison before and after.
- Row-count comparison.
- Backup path and restore-test result.

**Completion evidence (31 July 2026):**

- Guarded baseline tooling, its sealed version-13 manifest, and its tests were
  committed as `d3853ff` before any controlled baseline was run. The executed
  build identified itself as
  `1.0.0+d3853ff99d7c720f51ad0351f6d0fbf0422e7a31`.
- A full schema/data/routine/trigger/event dump was retained at
  `Backups/P2-05/personal_business_manager_restore_test_20260731_093351Z.sql`
  (70,678 bytes; SHA-256
  `ea29875061c80a98483a8da7249565d7776cf977cf782eadb40939bffb0c8f0f`).
  The exact hashed dump restored successfully to the disposable
  `pbm_p205_committed_copy_20260731` database.
- Read-only preflight verified the approved schema fingerprint
  `7a85fdf6b3c6bd5d4a2d5ba1f47c33af24f5a46714b89a07939b19a24fb79b6f`
  across 959 normalized metadata records, 31 application tables, and 116
  checks. It also validated required seeds, foreign keys, check constraints,
  row integrity, and the baseline-eligible history state.
- The committed-build rehearsal registered exactly versions `1` through `13`.
  Its log states for every version that no migration `Up()` method was
  executed. `status` and direct history inspection confirmed 13 rows with
  minimum version `1`, maximum version `13`, and no gaps.
- The before/after data fingerprint was
  `6e620fcec64f25cdc2a7638496fd697bee2a5fd4062837327ada4671566987cb`
  on both sides. It covers exact row counts for all 31 tables and 14 financial
  aggregates. Both snapshots contained 39 rows in total; all financial
  aggregates were zero. The application-schema fingerprint was also unchanged.
- A repeated rehearsal registration refused safely because history was no
  longer empty. The separately invoked `migrate` command found no migration
  above version `13` pending.
- Disposable evidence logs are
  `committed_copy_baseline_d3853ff.log` (SHA-256
  `15a0b86c484b4d129ad70b9c0cb1633f161f49c9705e6d5056e5db22e61497dc`)
  and `committed_copy_postverify_d3853ff.log` (SHA-256
  `d66272f679043328bd4e307795eeb1c10f61397c6f403c2be141ca081ba05a9e`).
  They are retained under the ignored `Backups/P2-05` directory and contain no
  credentials or connection strings.
- Only after that rehearsal passed, a fresh live backup was retained at
  `Backups/P2-05/personal_business_manager_pre_baseline_20260731_093647Z.sql`
  (70,678 bytes; SHA-256
  `52a52b91f2b7d6d88da32d0310100e02a1d3d110b2e8b0ffc80bacaf147258eb`).
- The real `personal_business_manager` database then passed read-only
  preflight and the same guarded baseline. Live history contains exactly
  versions `1` through `13`; `schema_information` contains its singleton at
  version `13` with verification and update timestamps. Post-baseline
  `status`, `verify`, direct read-only inspection, and separate `migrate` all
  passed with no pending migration and unchanged schema/data fingerprints.
- Live evidence logs are
  `live_baseline_d3853ff_20260731_093647Z.log` (SHA-256
  `0fc1847aadfdbfec883d961a1e46ceea57cf31dfca4ea67fc13898c071c66647`)
  and `live_postverify_d3853ff_20260731_093647Z.log` (SHA-256
  `e8ba5b95982f054e19f545da7de82a66ad8733f4ad79e9646b05555adc5a067b`).
- All three disposable databases were removed after evidence capture. Both
  backups and all evidence logs were retained outside Git.

---

## P2-06 — Add a real Dapper repository contract and implementation

**Priority:** Critical  
**Blocks Phase 3:** Yes for full Phase 2 completion  
**Estimated size:** Medium

The connection health check is not a repository read/write test.

Implement one small, architecture-compliant repository.

**Recommended option: application settings**

Create in Core:

```text
Application/Contracts/IApplicationSettingRepository.cs
Application/Dtos/ApplicationSettingDto.cs
```

Create in Infrastructure:

```text
Database/Repositories/ApplicationSettingRepository.cs
```

**Required behaviour:**

- [x] Read by `setting_key`.
- [x] Insert a test/development setting.
- [x] Update it using parameterised Dapper SQL.
- [x] Delete or roll back the test change in integration tests.
- [x] Use explicit columns.
- [x] Use async methods.
- [x] Accept cancellation tokens.
- [x] Use a new short-lived connection per operation.
- [x] Do not expose SQL to WinForms.
- [x] Do not add Phase 3 settings business logic yet.

**Alternative:**

Use another harmless existing foundation table, provided the audit test proves both read and write without modifying real user/business records.

**Verification:**

- [x] Repository read test passes.
- [x] Repository write test passes in a dedicated test database.
- [x] No SQL exists in WinForms.
- [x] Dapper is actually used.
- [x] Test cleanup is reliable.

**Completion evidence (31 July 2026):**

- Core now owns the focused `IApplicationSettingRepository` contract and
  `ApplicationSettingDto`; neither depends on Infrastructure, Dapper, or UI
  types.
- Infrastructure implements the contract with Dapper in
  `Database/Repositories/ApplicationSettingRepository.cs` and registers it as
  a transient dependency. Read, insert, update, and delete operations use
  explicit columns, named parameters, a 30-second command timeout, async APIs,
  and the supplied cancellation token.
- Every operation creates, opens, and asynchronously disposes a separate
  `MySqlConnection`. No connection or transaction is shared between calls.
- The repository performs persistence only. Value parsing, validation,
  settings workflows, auditing, and other Phase 3 behavior were deliberately
  not added.
- A source scan found zero SQL statement matches in the WinForms project.
  Dapper usage is directly exercised through `QuerySingleOrDefaultAsync`,
  `QuerySingleAsync`, `ExecuteAsync`, and `CommandDefinition`.
- Integration tests use `PBM_TEST_CONNECTION_STRING`. The MariaDB tests are
  visibly skipped when it is absent and reject unsafe targets. P2-07 now
  provides the reusable exact-target guard and migration-driven reset harness.
- Final database proof used the disposable
  `pbm_p206_repository_test_20260731_100456` database built through migrations
  `1` through `13`. Repository operations ran through a generated, temporary
  CRUD-only account rather than the MariaDB administrator account.
- The read test retrieved the seeded `locale = en-GB` row. The write test
  inserted a uniquely keyed setting, read it, updated a quote-containing value
  through parameterised SQL, read the new value, and deleted the row in a
  `finally` block.
- Post-test inspection found all 18 seed settings, zero matching test rows, and
  intact migration history `1` through `13`. The disposable database and
  account were then dropped; direct inspection found zero remaining objects.
- The full database-enabled suite passed 36 of 36 tests with zero failures and
  zero skips: 1 Core test and 35 Infrastructure/integration tests. A normal run
  without `PBM_TEST_CONNECTION_STRING` also passes while reporting the two
  database-dependent tests as skipped rather than touching another database.
- No supplied or generated password was written to source, documentation,
  test output, or Git, and the real development database was not modified.

---

## P2-07 — Create a safe MariaDB integration-test environment

**Priority:** Critical  
**Blocks Phase 3:** Yes for full Phase 2 completion  
**Estimated size:** Medium

- [x] Define a separate test database name.
- [x] Ensure tests refuse to run when the database name is the normal development database.
- [x] Use a separate test connection variable, for example:

```text
PBM_TEST_CONNECTION_STRING
```

- [x] Add a guard that rejects:
  - `personal_business_manager`;
  - production-like names;
  - missing test marker.
- [x] Create/reset the test database through the migration runner.
- [x] Do not use `root` as the application runtime account.
- [x] Allow a migration/admin account only for test database setup if necessary.
- [x] Ensure tests clean up inserted records.
- [x] Consider Testcontainers later if it works reliably in the environment.

**Evidence required:**

- Test configuration documentation.
- Guard test.
- No real application data modified by test execution.

**Completion evidence (31 July 2026):**

- The fixed local target is `personal_business_manager_test`; the runtime and
  migration variables are `PBM_TEST_CONNECTION_STRING` and
  `PBM_TEST_MIGRATION_CONNECTION_STRING` respectively.
- Shared guards require localhost, the exact test database, the exact account
  for the connection role, and an `_test` suffix. They reject the normal
  development database, production-like tokens, other test names, remote
  hosts, missing values, `root`, and crossed runtime/migration accounts.
- The migration tool now provides an explicit `reset-test` command. It accepts
  only the test migration variable, requires the exact confirmation
  `RESET TEST DATABASE personal_business_manager_test`, takes an advisory
  reset lock, drops/recreates only that database, applies the normal migration
  runner, and performs full post-migration verification. There is no force
  option.
- An intentional unsafe command pointed the test migration variable at
  `personal_business_manager`. It was refused before reset with exit code `2`;
  direct inspection confirmed the live 18 settings and migration history
  `1` through `13` were unchanged.
- Dedicated localhost accounts were created with independently generated
  random passwords. `personal_business_test_app` has only `SELECT`, `INSERT`,
  `UPDATE`, and `DELETE`. `personal_business_test_migrator` has only the
  documented database-scoped data/schema privileges and no global
  administration or `GRANT OPTION`. Neither test role uses `root`.
- The approved database was reset from empty through migrations `1` through
  `13` and matched the version-13 schema/data fingerprints: 31 application
  tables, 959 metadata records, 116 checks, and 39 seed rows.
- The database-enabled suite passed 59 of 59 tests with no failures or skips.
  Runtime evidence identified `personal_business_test_app@localhost`, retained
  all 18 seeded settings and exact history `1` through `13`, and found zero
  repository test rows after cleanup.
- Configuration, first-time grants, reset/test commands, safety behavior,
  cleanup rules, credential handling, and the deferred Testcontainers decision
  are documented in
  `docs/operations/mariadb_integration_test_environment.md`.
- Generated credentials were not written to the repository. The normal
  development database was never used for test reads or writes.

---

## P2-08 — Replace empty template tests with meaningful tests

**Priority:** Critical  
**Blocks Phase 3:** Yes for full Phase 2 completion  
**Estimated size:** Medium

### Core unit tests

Using the approved examples from P1-06, add tests for:

- [x] Monetary rounding.
- [x] VAT after discount.
- [x] Percentage discount.
- [x] Fixed discount.
- [x] Invoice totals.
- [x] Payment status.
- [x] Time rounding.
- [x] Duration calculation.
- [x] Net-worth calculation.
- [x] Asset/liability sign handling.
- [x] Profit estimate.
- [x] Tax-reserve estimate.

These may require adding small pure Core calculation classes. Do not add UI or database dependencies to Core.

### Infrastructure integration tests

Add tests for:

- [x] MariaDB connection factory.
- [x] Database health service.
- [x] Migration application to an empty test database.
- [x] Baseline handling for an existing-schema copy.
- [x] Dapper repository read.
- [x] Dapper repository write.
- [x] Repository cancellation support.
- [x] At least one critical unique constraint.
- [x] At least one foreign-key restriction.
- [x] Optimistic concurrency where foundation code exists.

**Cleanup:**

- [x] Delete `UnitTest1.cs` template tests.
- [x] Give every test a descriptive name.
- [x] Ensure tests fail for the correct reason.

**Verification:**

```powershell
dotnet test PersonalBusinessManager.slnx
```

- [x] All meaningful tests pass.
- [x] No empty placeholder tests remain.
- [x] Tests cannot target the real database.

**Completion evidence (31 July 2026):**

- Six pure calculation components were added to Core with no UI, database,
  Dapper, or MariaDB dependency: monetary rounding; invoice line/totals;
  settlement status; duration/time rounding; net worth; and profit/tax-reserve
  estimates.
- The Core suite now contains 38 passing cases. It verifies all eight approved
  midpoint examples, percentage and fixed discounts, VAT after discount,
  VAT-inclusive derivation, stored-line invoice totals, settlement precedence,
  overpayments, exact duration, every approved time-rounding rule, exact-boundary
  behaviour, signed asset/liability treatment, the £9,600 net-worth example,
  £1,100 profit estimate, and £275/£0 tax-reserve examples.
- Validation tests assert the relevant parameter and exception for invalid
  discounts and duration, so they fail for the intended business-rule reason.
- MariaDB tests now open the approved database through the connection factory,
  exercise both configured and unconfigured health results, and prove an
  already-cancelled repository operation propagates cancellation.
- Database constraint tests prove duplicate `application_settings.setting_key`
  values fail with MariaDB error `1062` and an unknown
  `updated_by_user_id` fails with error `1452`. Unique test keys are deleted in
  `finally`; the foreign-key rejection leaves no inserted row.
- The baseline integration test temporarily empties migration history in the
  guarded test database, registers the existing version-13 schema without
  replaying migration `Up` methods, compares pre/post schema and data
  fingerprints, and restores the exact original migration history and metadata
  in `finally`.
- Database tests sharing the fixed MariaDB target are in one non-parallel test
  collection so the baseline metadata transition cannot overlap repository or
  constraint tests.
- Optimistic concurrency was assessed under the checklist's "where foundation
  code exists" condition. No versioned repository exists yet—the only current
  repository is for unversioned `application_settings`—so a behavioural test
  would test invented SQL rather than application foundation code. It remains a
  required test when the first `version_no` repository is introduced.
- Both `UnitTest1.cs` files were deleted. A source scan finds no `UnitTest1`,
  `Test1`, empty template test, or undescriptive replacement.
- With both guarded test variables configured, all 103 tests pass with no
  failures or skips: 38 Core and 65 Infrastructure/integration tests. Without
  credentials, 95 tests pass and the eight database-dependent tests are
  explicitly skipped.
- Post-test current-schema verification succeeds for both the test and normal
  development databases with the approved schema fingerprint
  `7a85fdf6b3c6bd5d4a2d5ba1f47c33af24f5a46714b89a07939b19a24fb79b6f`
  and 39-row data fingerprint
  `6e620fcec64f25cdc2a7638496fd697bee2a5fd4062837327ada4671566987cb`.
  This confirms cleanup completed and real application data was not modified.

---

## P2-09 — Complete shared theme infrastructure

**Priority:** High  
**Blocks Phase 3:** No, but should be complete before more forms are added  
**Estimated size:** Medium

Create:

```text
Theming/ThemePalette.cs
Theming/UiSpacing.cs
Theming/UiFonts.cs
Theming/UiDimensions.cs
Theming/ThemeManager.cs
Theming/ControlStyler.cs
Theming/DpiScaler.cs
```

`ThemePalette` already exists; retain and improve it rather than duplicating it.

**Implement:**

- [x] Shared spacing values.
- [x] Shared fonts and font sizes.
- [x] Form/background styling.
- [x] Button styling.
- [x] Input styling.
- [x] Label/heading styling.
- [x] DataGridView styling.
- [x] TabControl styling.
- [x] Focus and disabled states.
- [x] DPI-aware sizing.

**Refactor:**

- [x] Remove repeated font declarations where practical.
- [x] Remove repeated padding/spacing values where practical.
- [x] Avoid scattering hard-coded colours across pages and controls.

**Verification:**

- [x] Main shell still looks correct.
- [x] New controls consume shared theme values.
- [x] Scaling is checked at 100%, 125% and 150%.

**Completion evidence (31 July 2026):**

- `ThemePalette` now implements the complete approved colour-token table from
  `docs/design/dark_theme_system.md`; the former muted-text mismatch was
  corrected. A source scan finds no colour construction elsewhere in the
  WinForms project.
- `UiSpacing`, `UiFonts`, and `UiDimensions` centralise the approved spacing
  scale, application-lifetime font instances, control sizes, shell dimensions,
  borders, and common component dimensions. `Program` disposes the shared
  fonts when the application exits.
- `ThemeManager` applies safe defaults recursively to forms, pages, dynamically
  added controls, and designer-created standard controls. It recognises
  `IThemeAwareControl`, preserves explicit semantic appearances, and exposes a
  validation scan that reports unapproved background or text colours.
- `ControlStyler` provides focused styling for forms/dialogs, panels, labels,
  buttons, text/combo/date/numeric inputs, `DataGridView`, `TabControl`,
  tool strips, and context menus. It includes hover, pressed, keyboard-focus,
  read-only, selected, and disabled states without adding duplicate event
  handlers when a control is themed again.
- Grid styling uses approved dark header/row/alternate/selection colours,
  40 px headers, 36 px rows, full-row selection, and double buffering. Tabs use
  owner-drawn dark headers, selected indicators, and visible keyboard focus.
- `DpiScaler` centralises 96-DPI logical-pixel conversion for custom painting.
  The shell remains `AutoScaleMode.Dpi`; custom borders and selection indicators
  scale by the control's current `DeviceDpi`, and DPI changes preserve the
  sizes applied by WinForms rather than resetting them to 96-DPI values.
- `MainShellForm`, `DashboardPage`, `PlaceholderPage`, `DarkButton`, and
  `SummaryCard` were refactored to consume shared theme, font, spacing, and
  dimension values. The shell now uses the approved 224 px sidebar, 64 px
  header, 48 px timer strip, and 1100 x 700 minimum window, and Dashboard is
  visibly selected on first display.
- A dedicated Windows-targeted theme test project contains 13 passing cases.
  It verifies exact tokens, approved spacing/dimensions, DPI calculations,
  safe recursive/default/semantic styling, disabled states, dark buffered-grid
  configuration, owner-drawn tabs, and the absence of unthemed controls in the
  shell.
- The complete database-enabled solution suite passes all 116 tests: 38 Core,
  65 Infrastructure/integration, and 13 WinForms theme tests. With database
  credentials deliberately absent, 108 pass and only the eight guarded MariaDB
  tests skip.
- Full off-screen shell renders were inspected at 96, 120, and 144 DPI
  (100%, 125%, and 150%). Layout, typography, navigation selection, cards, and
  status content remained readable and unclipped at each required scale.

---

## P2-10 — Implement the minimum reusable themed controls

**Priority:** High  
**Blocks Phase 3:** Partly  
**Estimated size:** Medium

### Required now

- [x] `DarkButton`
- [x] `SummaryCard`
- [x] `DarkTextBox`
- [x] `DarkComboBox`
- [x] `DarkDateTimePicker`
- [x] `DarkDataGridView`
- [x] `DarkTabControl`
- [x] `PageHeader`
- [x] `FilterBar`
- [x] `StatusBadge`
- [x] `EmptyStatePanel`
- [x] `LoadingOverlay`
- [x] `ValidationMessage`
- [x] `ConfirmDialog`

### Reasonably deferred until first use

- [ ] `CurrencyTextBox` — can be completed before finance data entry.
- [ ] `DurationTextBox` — can be completed before time-entry editing.

**Control requirements:**

- [x] Theme values are centralised.
- [x] Keyboard focus is visible.
- [x] Disabled states are readable.
- [x] Designer support does not create analyzer errors.
- [x] Controls remain usable under DPI scaling.
- [x] DataGridView uses double buffering.
- [x] Empty/loading/error states are explicit.

**Verification:**

- [x] Add a development-only control gallery page or manual test form.
- [x] Verify controls visually at common scaling levels.
- [x] Remove the gallery from normal navigation if it is not intended for production.

**Completion evidence (3 August 2026):**

- Added the complete required-now control set: themed text, combo and date
  inputs; buffered grid and owner-painted tabs; reusable page header and filter
  bar; six-role status badges; explicit empty/error/loading/validation states;
  and severity-aware confirmation dialogs with optional exact typed
  confirmation for dangerous actions.
- `DarkButton` now supports primary, secondary, ghost and danger variants,
  compact/standard/large sizing, readable disabled/hover/pressed/focus states,
  and the selected navigation treatment already used by `MainShellForm`.
  `SummaryCard` now also has a designer-safe parameterless constructor.
- Theme colours, typography, spacing, component dimensions and semantic-role
  mappings remain centralised. Typed controls implement `IThemeAwareControl`,
  are parameterless/designer constructible, expose meaningful accessibility
  text or roles, and do not create new analyzer warnings.
- `DarkDataGridView` uses approved row/header/selection styles and direct double
  buffering. `DarkComboBox` owner-draws its list. `DarkDateTimePicker` renders a
  fully dark date field in the approved `dd/MM/yyyy` form and uses a native
  `MonthCalendar` only for the operating-system popup, as allowed by the
  documented native-control limitation policy.
- `ThemeControlGalleryForm` provides three development-only pages covering
  inputs/actions, data/status and states/feedback. It is intentionally absent
  from normal application navigation and is marked development-only for code
  browsing and designer purposes.
- Automated render coverage captures all three gallery pages and the dangerous
  typed-confirmation dialog at 96, 120 and 144 DPI (100%, 125% and 150%). The
  captures were inspected for clipping, dark surfaces, explicit states,
  readable controls and visible focus/error treatments.
- The Windows UI suite now contains 41 passing tests, including 28 P2-10 cases
  for designer construction, interaction states, semantic mappings, typed
  confirmation, gallery composition, theme validation, double buffering and
  multi-scale rendering.
- The complete credential-free solution run passes 136 tests: 38 Core, 57
  non-database Infrastructure/integration and 41 WinForms tests. The eight
  explicitly guarded MariaDB cases skip when the local test database is not
  available.
- `CurrencyTextBox` and `DurationTextBox` remain deliberately unchecked and
  deferred under this task's approved “until first use” rule. They must be
  completed before finance-entry and time-entry editing respectively and must
  follow `docs/design/dark_theme_system.md` when introduced.

---

## P2-11 — Complete the main shell infrastructure

**Priority:** High  
**Blocks Phase 3:** No, but required for full Phase 2 completion  
**Estimated size:** Medium

The current shell already has the main structure. Add the missing planned shell features.

- [x] Collapsible sidebar.
- [x] Compact collapsed navigation state.
- [x] Current-user menu placeholder or Phase 3-ready host control.
- [x] Backup-status indicator abstraction.
- [x] Non-blocking notification area.
- [x] Loading overlay.
- [x] Safe page-loading API.
- [x] Navigation state preservation where appropriate.
- [x] Clean page disposal.
- [x] No duplicate event subscriptions.
- [x] Persistent timer-strip host remains visible.
- [x] Minimum window size remains usable.
- [x] Keyboard navigation works.

**Current-user menu rule:**

Phase 2 may provide the control and placeholder state. Authentication behaviour belongs to Phase 3.

**Verification:**

- [x] Click every sidebar destination.
- [x] Collapse and expand the sidebar.
- [x] Confirm title and breadcrumbs update.
- [x] Confirm pages are disposed correctly.
- [x] Confirm notification and loading controls do not block the UI unnecessarily.

**Completion evidence (3 August 2026):**

- `MainShellForm` now implements the approved 224 px expanded and 64 px compact
  sidebar states. It automatically enforces compact mode below the 1180 px
  responsive threshold, changes content padding from 24 px to 16 px
  horizontally, retains selected-state indicators, and presents centred compact
  labels with full accessible names and tooltips.
- All fourteen approved permanent routes are registered through
  `ShellPageDefinition`. `NavigateAsync` checks page navigation guards, cancels
  obsolete work, honours external cancellation, reports safe retry states,
  updates title/breadcrumb/selection only for the current request, and moves
  focus into the loaded page.
- `IShellNavigationStatefulPage` preserves supported filter/list state by route.
  Outgoing pages are removed and disposed exactly once, newly created pages from
  obsolete requests are disposed, cancellation sources are replaced and
  disposed, and repeated theme application does not add navigation handlers.
- The P2-10 `LoadingOverlay` is hosted only over page content. Sidebar, header,
  notifications and the persistent 48 px timer host remain available while a
  cancellable page load is active. Page failures show an explicit error/retry
  state and a safe reference without exposing exception text or stack traces.
- `NotificationArea` provides a top-right stack of explicit information,
  success, warning and error messages. It does not take focus when shown,
  supports optional actions and dismissal, permits automatic dismissal only for
  information/success messages, and keeps warnings/errors until dismissed.
- `BackupStatusIndicator` exposes not-configured, healthy, in-progress, warning
  and failed states with textual status and semantic styling. Selecting it opens
  the approved Backups route. `CurrentUserMenu` supplies disabled Lock,
  Account/Security and Sign-out placeholders until Phase 3 provides a session;
  no authentication behaviour was implemented early.
- Sidebar destinations remain in the approved concise order. Up/Down keys move
  through navigation entries, Tab/Enter remain available, and `Ctrl+B` toggles
  expanded/compact mode when the responsive minimum does not require compact
  mode.
- The WinForms suite now contains 61 passing tests, including 20 P2-11 cases.
  They cover all destinations, headers/breadcrumbs, manual and automatic
  collapse, compact accessibility, current-user and backup hosts,
  notifications, navigation guards/state/disposal, cancellable and obsolete
  loading, failure/retry presentation, duplicate-handler prevention, keyboard
  movement, and render validation.
- The complete credential-free solution run passes 156 tests: 38 Core, 57
  non-database Infrastructure/integration and 61 WinForms tests. The eight
  explicitly guarded MariaDB cases skip when the local test database is not
  available.
- Expanded, compact and content-loading shells were rendered and inspected at
  96, 120 and 144 DPI. The nine captures keep text, focus, navigation, header
  controls, notifications, page content and the timer strip readable and
  unclipped, with no default light application controls.

---

## P2-12 — Add the list/search/filter/paging foundation

**Priority:** High  
**Blocks Phase 3:** No; blocks efficient Phase 4 list screens  
**Estimated size:** Medium

Create reusable foundation types without implementing customer/job features early.

**Core foundation:**

- [ ] `PagedResult<T>`.
- [ ] Paging request model.
- [ ] Sort direction.
- [ ] Base filter conventions.
- [ ] Maximum page-size validation.
- [ ] Cancellation-token conventions.

**WinForms foundation:**

- [ ] `FilterBar`.
- [ ] Debounced search helper, approximately 250–400 ms.
- [ ] Cancellation of obsolete requests.
- [ ] Paging control.
- [ ] Loading state.
- [ ] Empty state.
- [ ] Error/retry state.
- [ ] `DarkDataGridView`.

**Infrastructure foundation:**

- [ ] Keyset-pagination SQL conventions.
- [ ] Explicit deterministic sorting.
- [ ] Lightweight list projections.
- [ ] Command timeout convention.

**Verification:**

- [ ] A small test/demo query can page without loading unlimited rows.
- [ ] Search cancellation works.
- [ ] No UI freeze during async loading.
- [ ] No feature-specific SQL appears in WinForms.

---

## P2-13 — Confirm connection-string protection plan

**Priority:** High  
**Blocks Phase 3:** No; Phase 3 will complete credential protection  
**Estimated size:** Small

The user environment variable is acceptable as a temporary development mechanism, but it is not the final protected storage method.

- [ ] Document that `PBM_CONNECTION_STRING` is development-only.
- [ ] Plan Windows Credential Manager or DPAPI current-user storage.
- [ ] Confirm the string is never logged.
- [ ] Confirm it is not committed.
- [ ] Confirm startup logs only present/absent state.
- [ ] Add a secret-scanning step before commits/releases.

**Create:**

```text
docs/security/development_credentials.md
```

**Verification:**

- [ ] Repository search finds no committed password.
- [ ] Logs contain no connection string.
- [ ] The dedicated DB account is used.

---

## P2-14 — Review and resolve analyzer warnings

**Priority:** Medium  
**Blocks Phase 3:** Only if a warning represents a real defect  
**Estimated size:** Small to medium

Current build reports 12 analyzer warnings.

- [ ] Capture the exact warning list.
- [ ] Classify each warning:
  - real defect;
  - design issue;
  - generated/designer noise;
  - acceptable exception.
- [ ] Fix project-authored warnings where practical.
- [ ] Avoid suppressing warnings globally without justification.
- [ ] Document intentional suppressions close to the affected code.
- [ ] Rebuild after each group of fixes.

**Target:**

```text
0 errors
0 unexplained project-authored warnings
```

**Verification:**

```powershell
dotnet build PersonalBusinessManager.slnx
```

- [ ] Any remaining warnings are documented.
- [ ] No warning indicates broken designer serialization, disposal or nullable handling.

---

## P2-15 — Clean repository and Git state

**Priority:** Medium  
**Blocks Phase 3:** No, but required before declaring the phase complete  
**Estimated size:** Small

- [ ] Review modified files.
- [ ] Review untracked `.resx` files.
- [ ] Keep required WinForms resource files.
- [ ] Remove accidental or obsolete files.
- [ ] Confirm `.gitignore` excludes:
  - `.vs/`
  - `bin/`
  - `obj/`
  - logs;
  - local configuration;
  - secrets;
  - user-specific IDE files.
- [ ] Run a repository secret search.
- [ ] Confirm no database password appears in Git history.
- [ ] Create coherent commits for the completed Phase 1 and Phase 2 work.
- [ ] End with a clean working tree.

**Verification:**

```powershell
git status
```

Expected:

```text
nothing to commit, working tree clean
```

---

# Final verification checklist

## Build

- [ ] `dotnet restore` succeeds.
- [x] `dotnet build PersonalBusinessManager.slnx` succeeds.
- [x] No errors.
- [x] No unexplained project-authored warnings.

## Tests

- [x] `dotnet test PersonalBusinessManager.slnx` succeeds.
- [x] Empty template tests have been removed.
- [x] Meaningful Core unit tests pass.
- [x] Meaningful MariaDB integration tests pass.
- [x] Test database guards pass.

## Runtime

- [ ] Application starts normally.
- [x] Main dark shell displays.
- [x] Sidebar collapses and expands.
- [x] Every navigation item works.
- [x] Title and breadcrumbs update.
- [ ] Database reports connected.
- [ ] Runtime DB identity is not root.
- [ ] Logs are created.
- [ ] Logs contain no credentials.
- [x] Application closes cleanly.

## Database

- [ ] Dedicated runtime account is active.
- [ ] Runtime grants are least privilege.
- [ ] Migration/admin access is separate where practical.
- [ ] FluentMigrator version table exists.
- [x] Empty test database is created through migrations.
- [ ] Existing schema baseline has been tested.
- [ ] Existing development database is safely baselined.
- [x] Sample Dapper repository read/write passes.

## Documentation and approvals

- [ ] MVP scope approval recorded.
- [ ] Schema approval recorded.
- [ ] Navigation approval recorded.
- [ ] Workflow-code reference complete.
- [ ] Wireframes complete.
- [ ] Financial examples approved.
- [ ] Migration order approved.
- [ ] Migration baseline strategy approved.
- [ ] MariaDB version strategy recorded.
- [ ] Development credential strategy recorded.

---

# Phase gate

Phase 1 is complete only when all of the following are true:

- [ ] Scope is formally approved.
- [ ] Workflow/code values are complete.
- [ ] Schema review is recorded.
- [ ] Wireframes exist and are approved.
- [ ] Dark-theme design is complete.
- [ ] Worked calculation examples exist and are approved.
- [ ] Migration order and baseline policy are approved.

Phase 2 is complete only when all of the following are true:

- [ ] Solution structure and dependency direction remain correct.
- [ ] Dependency injection and logging work.
- [ ] Dedicated MariaDB runtime account is in use.
- [x] Dapper is used by a real repository.
- [ ] FluentMigrator is configured.
- [x] Initial migrations build an empty test database.
- [ ] Existing schema is safely baselined.
- [x] Meaningful unit and integration tests pass.
- [x] Main dark shell infrastructure is complete.
- [x] Minimum reusable themed controls exist.
- [ ] List/search/paging foundation exists.
- [ ] Build warnings are resolved or justified.
- [ ] Git working tree is clean.

## Final decision

```text
Phase 1 gate: PASS / FAIL
Phase 2 gate: PASS / FAIL
Ready to begin Phase 3: YES / NO / YES WITH CONDITIONS
```

**Conditions or notes:**

```text



```
