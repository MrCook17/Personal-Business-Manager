# Migration Baseline Strategy

**Project:** Personal Business Manager  
**Decision:** P1-07 — Approve the migration order and baseline policy  
**Document status:** Approved Phase 1 migration decision  
**Decision date:** 29 July 2026  
**Owner:** Charlie Cook  
**Repository path:** `docs/decisions/migration_baseline_strategy.md`  
**Database:** MariaDB  
**Migration framework:** FluentMigrator  
**Approved initial baseline version:** `13`  
**Related documents:**

```text
personal_business_management_application_final_plan.md
docs/decisions/schema_review.md
docs/personal_business_management_application_schema.sql
docs/schema_review_live_amendments.sql
docs/reference/workflow_codes.md
```

---

# 1. Executive decision

The current approved 31-table MariaDB schema is the initial database baseline.

The initial schema will be represented by thirteen FluentMigrator migrations:

```text
0001 through 0013
```

The exact FluentMigrator baseline version is:

```text
13
```

New empty databases must be created by executing migrations `1` through `13`.

The existing development database must **not** execute the `Up` methods for migrations `1` through `13`, because the equivalent approved schema and seed data already exist.

Instead, a dedicated migration tool will provide a controlled one-time:

```text
baseline-existing
```

operation. This operation will:

1. verify the target database matches the approved version-13 schema;
2. require a successful backup and restore test;
3. create FluentMigrator’s migration-history table;
4. record migrations `1` through `13` as already applied;
5. update the application-level schema information to version `13`;
6. verify that no application table, constraint, index or business data changed.

Baselining is never automatic.

Normal WinForms startup must not:

- create the migration-history table;
- register a baseline;
- execute pending migrations;
- request schema-altering database privileges;
- silently repair a schema mismatch.

After version `13`, every schema change uses a new migration:

```text
14, 15, 16, ...
```

Released migrations are immutable.

---

# 2. Reasons for this approach

This strategy is approved because:

- the existing development schema was created manually before FluentMigrator was introduced;
- the schema has now passed P1-03 review;
- the live database and amended bootstrap were reported as structurally identical;
- the approved baseline contains 31 application tables and 116 enforced checks;
- replaying table-creation migrations against the existing database would fail or risk unintended changes;
- marking migrations as applied without verification would hide schema drift;
- maintaining separate upgrade SQL indefinitely would create two competing migration systems;
- a controlled baseline preserves the existing database while making future migrations authoritative.

The standalone:

```text
docs/schema_review_live_amendments.sql
```

remains historical evidence of the pre-baseline repair.

It is **not** migration `14`, and it must not be executed against new empty databases. Migrations `1` through `13` create the already-amended final baseline directly.

---

# 3. Source-of-truth hierarchy

Before the baseline is registered, the approved schema sources are:

1. `docs/decisions/schema_review.md`;
2. `docs/personal_business_management_application_schema.sql`;
3. the approved P1-02 workflow codes;
4. the final development plan;
5. the tested live database.

After migrations `1` through `13` have been implemented, tested and committed:

1. FluentMigrator migrations become the authoritative executable schema history;
2. `schema_migrations` becomes the authoritative applied-version history in each database;
3. `schema_information` remains an application compatibility summary;
4. the bootstrap SQL remains an independently reviewable reference and parity target;
5. the one-time amendment script remains historical evidence only.

A future developer must not edit the bootstrap SQL and assume an existing database has changed.

Every post-baseline schema change requires a new FluentMigrator migration.

---

# 4. Approved version-number policy

## 4.1 FluentMigrator versions

Use monotonically increasing integer versions:

```text
1
2
3
...
13
14
...
```

Example:

```csharp
[Migration(1, "Create users and security tables")]
public sealed class Migration_0001_CreateUsersAndSecurity : Migration
{
    public override void Up()
    {
        // ...
    }

    public override void Down()
    {
        // Safe development/test rollback where practical.
    }
}
```

## 4.2 File and class naming

Use four-digit display padding for filenames and class names:

```text
Migration_0001_CreateUsersAndSecurity.cs
Migration_0002_CreateSettingsAndAudit.cs
...
Migration_0013_SeedInitialApplicationSettings.cs
Migration_0014_DescriptionOfLaterChange.cs
```

The `[Migration(...)]` value remains the unpadded integer:

```text
1 through 13
```

## 4.3 Exact baseline

The approved baseline includes all migrations through:

```text
Migration version 13
```

The existing database must never be baselined to:

- `12`;
- a date-based approximation;
- the latest version found at runtime;
- a version greater than `13`;
- an arbitrary version supplied without validation.

If migrations `14` or later exist when the baseline operation runs:

1. register only versions `1` through `13`;
2. report `14+` as pending;
3. require a separate explicit migration operation to apply them.

## 4.4 Why date-based migration numbers are not used

A value such as:

```text
202607290013
```

would be valid for FluentMigrator but does not fit the current application-level:

```text
schema_information.schema_version INT UNSIGNED
```

Using integer versions `1`, `2`, `3` and so on keeps:

- FluentMigrator history;
- `schema_information.schema_version`;
- documentation;
- diagnostics;

aligned without unnecessary conversion.

---

# 5. Migration-history table

## 5.1 Approved table

Configure FluentMigrator to use a lowercase `snake_case` history table:

```text
schema_migrations
```

Logical columns:

```text
version
applied_on_utc
description
```

Recommended logical definition:

```text
version         BIGINT NOT NULL
applied_on_utc  DATETIME(6) NOT NULL
description     VARCHAR(1024) NULL
```

The migration framework may require a compatible index or primary-key shape.

The exact FluentMigrator metadata implementation must preserve:

- one row per applied migration version;
- uniqueness of `version`;
- UTC timestamps;
- readable descriptions;
- lowercase `snake_case` names.

## 5.2 Relationship to `schema_information`

`schema_information` and `schema_migrations` serve different purposes.

### `schema_migrations`

Authoritative migration history:

```text
Which versioned migrations have been applied?
```

### `schema_information`

Application compatibility summary:

```text
What schema version does the application consider current and verified?
```

`schema_information` must not replace FluentMigrator’s history table.

After a successful migration or baseline:

```text
schema_information.schema_version
```

must equal the greatest successfully applied migration version.

For the initial baseline:

```text
schema_information.schema_version = 13
```

`last_verified_utc` is updated only after post-operation verification succeeds.

---

# 6. Approved initial migration order

## 6.1 Migration 0001 — Users and security

```text
Version: 1
Name:    0001_create_users_and_security
```

Creates:

```text
users
password_recovery_codes
```

Includes:

- primary keys;
- essential columns;
- security-related foreign keys;
- required uniqueness;
- essential security checks needed for valid rows.

Does not seed a user or password.

## 6.2 Migration 0002 — Settings, schema information, audit and backups

```text
Version: 2
Name:    0002_create_application_settings_and_audit
```

Creates:

```text
application_settings
schema_information
audit_records
backup_records
```

Also creates the singleton application-level schema-information row where required for migration progress tracking.

System-originated audit events must permit:

```text
audit_records.user_id = NULL
```

## 6.3 Migration 0003 — Customers, contacts and addresses

```text
Version: 3
Name:    0003_create_customers_contacts_and_addresses
```

Creates:

```text
customers
customer_contacts
customer_addresses
```

Includes customer archive-state fields and relationships.

## 6.4 Migration 0004 — Jobs, tasks and time tracking

```text
Version: 4
Name:    0004_create_jobs_tasks_and_time_tracking
```

Creates:

```text
jobs
active_timers
time_entries
tasks
```

Uses exact duration fields:

```text
raw_duration_seconds
rounded_duration_seconds
```

Does not create a recurring-task placeholder.

## 6.5 Migration 0005 — Financial account types and accounts

```text
Version: 5
Name:    0005_create_financial_account_types_and_accounts
```

Creates:

```text
financial_account_types
financial_accounts
```

Preserves business/personal scope separation and asset/liability classification.

## 6.6 Migration 0006 — Account snapshots, applications and contributions

```text
Version: 6
Name:    0006_create_account_snapshots_applications_and_contributions
```

Creates:

```text
financial_account_balance_snapshots
financial_account_applications
financial_account_contributions
```

## 6.7 Migration 0007 — Invoice sequences, invoices and lines

```text
Version: 7
Name:    0007_create_invoice_sequences_invoices_and_lines
```

Creates:

```text
invoice_number_sequences
invoices
invoice_lines
```

Includes draft/finalised structure and credit references.

Self-credit prevention remains an application-service invariant because MariaDB 10.4 cannot enforce the attempted auto-increment self-reference check directly.

## 6.8 Migration 0008 — Invoice time links and payments

```text
Version: 8
Name:    0008_create_invoice_time_links_and_payments
```

Creates:

```text
invoice_time_entries
invoice_payments
```

Stores:

```text
billed_seconds
```

and does not create:

```text
billed_minutes
```

## 6.9 Migration 0009 — Expenses

```text
Version: 9
Name:    0009_create_expense_categories_and_expenses
```

Creates:

```text
expense_categories
expenses
```

`expenses.payment_method_code` is required.

## 6.10 Migration 0010 — Attachments and link tables

```text
Version: 10
Name:    0010_create_attachments_and_link_tables
```

Creates:

```text
attachments
customer_attachments
job_attachments
expense_attachments
financial_account_attachments
financial_account_application_attachments
```

Attachment link tables use the approved cascade behaviour.

## 6.11 Migration 0011 — Core lookup seed data

```text
Version: 11
Name:    0011_seed_core_lookup_data
```

Seeds:

- approved financial-account types;
- the `Uncategorised` expense category;
- invoice and credit-note sequence rows;
- other immutable or controlled lookup rows approved by the schema review.

Rules:

- use stable code keys;
- preserve existing valid rows;
- do not reset user-modifiable values;
- never reduce invoice sequence values;
- never overwrite an existing configured invoice prefix.

## 6.12 Migration 0012 — Required indexes and constraints

```text
Version: 12
Name:    0012_create_required_indexes_and_constraints
```

Creates or confirms:

- remaining secondary indexes;
- closed-code checks;
- Boolean checks;
- archive and lifecycle checks;
- financial-total checks;
- invoice-line and discount checks;
- payment-reversal checks;
- required unique constraints;
- remaining foreign-key restrictions.

Constraints necessary for earlier migration inserts may be created earlier, but version `12` is responsible for final parity with the approved baseline.

## 6.13 Migration 0013 — Initial application settings

```text
Version: 13
Name:    0013_seed_initial_application_settings
```

Seeds approved non-sensitive application settings, including:

- locale;
- currency;
- theme;
- initial business/invoice defaults;
- conservative estimate defaults;
- backup defaults where approved.

Does not seed:

- database credentials;
- passwords;
- recovery codes;
- online-banking secrets;
- an administrator account.

At completion:

```text
schema_information.schema_version = 13
```

---

# 7. Migration implementation rules

## 7.1 General rules

Every migration must:

- have a unique version;
- have an `Up`;
- preserve lowercase `snake_case`;
- preserve `record_id`;
- use InnoDB;
- use `utf8mb4`;
- use explicit names for important constraints and indexes;
- contain no credentials;
- log its version and outcome safely;
- be tested against a disposable database;
- be committed before use outside an isolated developer test.

## 7.2 Released migrations are immutable

A migration is considered released when it has been:

- merged into the main development branch; and
- applied to any non-disposable development, test or production-like database.

After release:

- never edit its `Up`;
- never edit its `Down`;
- never reuse its version;
- never reorder it;
- never delete it.

Corrections use a new migration.

## 7.3 `Down` policy

A `Down` method is required by the migration class, but destructive rollback is not automatically approved.

For migrations `1` through `13`:

- safe reverse drops may be provided for disposable empty-database tests;
- the preferred reset for an empty disposable database is dropping and recreating the entire test database;
- do not run destructive `Down` methods against the existing development database;
- do not treat `Down` as a substitute for backup restore.

For later migrations:

- implement `Down` only where rollback is lossless, unambiguous and tested;
- otherwise throw a clear `NotSupportedException` or equivalent controlled failure;
- document restore or corrective-forward-migration recovery.

## 7.4 MariaDB DDL transaction limitation

MariaDB/MySQL DDL may perform implicit commits.

Therefore a multi-statement schema migration cannot always be treated as fully transactional.

Mitigations:

- keep each migration coherent and as small as practical;
- test the exact migration against a disposable restored copy;
- require a verified backup before non-disposable schema changes;
- perform explicit precondition checks;
- avoid hiding partial state through broad `IF NOT EXISTS`;
- verify postconditions before declaring success;
- stop immediately on failure;
- restore or apply a reviewed corrective migration rather than guessing.

## 7.5 Seed-data rules

Seed migrations must be deterministic.

Use stable natural/code keys for upsert behaviour.

Do not overwrite mutable user configuration on rerun.

Specifically:

- preserve invoice/credit-note prefixes;
- preserve sequence numbers and never reduce them;
- preserve intentionally modified settings;
- add missing required lookup rows;
- update an existing lookup only when an approved migration explicitly changes its definition.

---

# 8. Migration tool architecture

## 8.1 Dedicated tool

Use a dedicated console project:

```text
tools/PersonalBusinessManager.DatabaseMigrator/
```

This is preferred over placing schema administration inside the WinForms executable.

The tool references the Infrastructure migration assembly.

Recommended migration location:

```text
src/PersonalBusinessManager.Infrastructure/Database/Migrations/
```

## 8.2 Approved commands

The tool should provide explicit commands equivalent to:

```text
status
verify-baseline
migrate
baseline-existing
verify
```

Optional development/test commands:

```text
create-test-database
drop-test-database
list-migrations
```

Do not expose a casual production-style:

```text
migrate-down-all
```

command.

## 8.3 Command responsibilities

### `status`

Read-only.

Reports:

- target server and database using safe identifiers;
- current highest applied migration;
- all applied versions;
- pending versions;
- schema-information version;
- whether history and application version agree.

### `verify-baseline`

Read-only against the target application schema.

Verifies version-13 eligibility without writing migration history.

### `baseline-existing`

One-time, guarded write operation.

Registers versions `1` through `13` only after all eligibility checks pass.

### `migrate`

Applies pending migrations above the current registered version.

Requires explicit confirmation for a non-test database.

### `verify`

Runs post-migration structural and compatibility checks.

## 8.4 No startup migration

The normal WinForms application may perform a read-only compatibility check:

```text
Expected schema version
Actual schema version
Pending/incompatible indication
```

It must not run migrations.

If incompatible:

- stop write operations;
- show a safe maintenance message;
- direct the developer/operator to the migration tool;
- log safe version information;
- never request migration credentials.

---

# 9. Connection and privilege policy

## 9.1 Separate connections

Use separate protected connection settings:

```text
PBM_CONNECTION_STRING
PBM_MIGRATION_CONNECTION_STRING
PBM_TEST_CONNECTION_STRING
PBM_TEST_MIGRATION_CONNECTION_STRING
```

Purpose:

| Setting | Purpose |
|---|---|
| `PBM_CONNECTION_STRING` | Normal application runtime account. |
| `PBM_MIGRATION_CONNECTION_STRING` | Explicit local migration/admin operation. |
| `PBM_TEST_CONNECTION_STRING` | Integration-test runtime access. |
| `PBM_TEST_MIGRATION_CONNECTION_STRING` | Test database creation/migration access. |

## 9.2 Runtime account

The runtime account should normally have only required data privileges such as:

```text
SELECT
INSERT
UPDATE
DELETE
```

It must not require:

```text
CREATE
ALTER
DROP
INDEX
REFERENCES
GRANT OPTION
```

## 9.3 Migration account

The migration account may have schema-altering privileges restricted to:

```text
personal_business_manager.*
```

or the explicitly named test database.

It must not receive unnecessary global privileges or `GRANT OPTION`.

## 9.4 Secret handling

The migration tool must never:

- log a connection string;
- print a password;
- place a password in command history;
- commit credentials;
- include credentials in baseline evidence.

It may safely display:

- server host;
- database name;
- server version;
- current MariaDB account identity;
- migration version.

---

# 10. Empty-database creation policy

## 10.1 Required process

To create a new empty database:

1. create the empty database with approved charset/collation;
2. configure the migration connection;
3. run `status`;
4. confirm no application tables or migration versions exist;
5. execute `migrate`;
6. apply versions `1` through `13`;
7. execute `verify`;
8. compare against the approved application-schema manifest/bootstrap;
9. run integration tests.

## 10.2 Expected result

Ignoring the migration-history table, the result must match the approved version-13 application schema:

```text
31 application tables
116 enforced CHECK constraints
approved columns
approved indexes
approved foreign keys
approved unique constraints
approved lookup rows
approved initial settings
```

With FluentMigrator history included, the database will also contain:

```text
schema_migrations
```

Therefore the physical table count may be:

```text
32 total tables
```

while the approved application-domain/support schema remains:

```text
31 tables
```

## 10.3 Bootstrap SQL parity

The bootstrap SQL and migrations must produce equivalent application objects.

Exclude the migrator-owned:

```text
schema_migrations
```

table from the application-schema parity comparison.

Do not compare volatile metadata such as:

- table creation timestamps;
- auto-increment next values;
- migration applied timestamps;
- database file/internal identifiers.

---

# 11. Baseline eligibility for the existing database

The existing database is eligible for version-13 registration only when all checks pass.

## 11.1 Identity checks

Confirm:

- expected server;
- expected database name;
- expected local/development environment;
- expected MariaDB account;
- no accidental test or production target confusion.

Required explicit confirmation should include the database name:

```text
personal_business_manager
```

## 11.2 Migration-history checks

Before baseline:

```text
schema_migrations
```

must be absent or empty.

If it contains any row:

- do not baseline;
- report current history;
- investigate the existing migration state.

## 11.3 Schema checks

Verify the normalized baseline fingerprint for:

- application table names;
- storage engine;
- collation;
- columns;
- column types;
- nullability;
- defaults;
- generated/extra attributes;
- primary keys;
- unique constraints;
- foreign keys;
- update/delete rules;
- check constraints and clauses;
- indexes and ordered index columns.

Expected application schema:

```text
31 tables
116 enforced checks
```

The previously reported:

```text
756 normalized metadata records
```

may be used as supporting evidence, but the command must compare actual metadata content rather than trusting the count alone.

## 11.4 Obsolete-object checks

The following must be absent:

```text
time_entries.rounded_duration_minutes
invoice_time_entries.billed_minutes
tasks.recurrence_definition_id
```

The following must be present:

```text
time_entries.rounded_duration_seconds
nullable audit_records.user_id
required expenses.payment_method_code
approved checks and constraints
```

## 11.5 Seed checks

Verify required identities rather than overwriting values.

Confirm:

- approved financial-account type codes exist;
- required sequence rows exist;
- `Uncategorised` expense category exists;
- required initial setting keys exist;
- no seeded administrator exists;
- invoice sequence values are valid;
- existing invoice prefixes are preserved;
- required code values use approved spelling.

Mutable setting values do not have to equal their original defaults if they remain valid.

## 11.6 Data-integrity checks

Before baseline:

- all foreign keys are valid;
- all check constraints are satisfied;
- no duplicate unique-key data exists;
- no second active timer exists for a user;
- no time entry is linked to more than one invoice;
- financial totals satisfy stored equations;
- archive/status timestamp rules are valid;
- all closed-code values are approved;
- no pending unreviewed schema repair remains.

## 11.7 Version-information checks

Before baseline, `schema_information` must:

- contain exactly the approved singleton row;
- contain a valid schema version;
- not claim a version above `13`.

The baseline command will set:

```text
schema_information.schema_version = 13
```

only after migration-history registration succeeds.

---

# 12. Approved baseline procedure

## 12.1 Phase A — Repository preparation

Before any baseline execution:

1. correct the `.gitignore` rule that currently ignores `docs/`;
2. commit the approved schema review;
3. commit the approved bootstrap SQL;
4. commit the one-time amendment script;
5. implement migrations `1` through `13`;
6. implement the migration tool and baseline guard;
7. commit the approved baseline manifest or comparison logic;
8. build and test the solution;
9. ensure the Git working tree is clean.

Do not baseline a database against uncommitted migration code.

## 12.2 Phase B — Build a reference database

1. create a disposable empty MariaDB database;
2. apply migrations `1` through `13`;
3. run schema verification;
4. compare it with the approved bootstrap schema;
5. generate/confirm the normalized version-13 application-schema fingerprint;
6. run seed verification;
7. run integration tests;
8. record the tool and migration commit identifier.

## 12.3 Phase C — Create and verify backup

1. create a full backup of the existing development database;
2. include a timestamp in the filename;
3. calculate a SHA-256 hash;
4. record file size;
5. restore it into a disposable database;
6. verify the restored row counts and key financial totals;
7. retain the backup until baseline and subsequent migrations are verified.

A backup that has not been restored successfully is not sufficient evidence.

## 12.4 Phase D — Baseline a restored disposable copy

Against the restored copy:

1. run `status`;
2. run `verify-baseline`;
3. ensure the target matches version `13`;
4. run `baseline-existing --to 13`;
5. run `status` again;
6. confirm versions `1` through `13` are recorded;
7. confirm no migration `Up` method ran;
8. confirm no application schema object changed;
9. confirm no application row count or financial total changed;
10. confirm only expected migration metadata/schema-information values changed;
11. run application integration tests against the copy.

## 12.5 Phase E — Baseline the real development database

Only after the restored-copy test passes:

1. stop the WinForms application;
2. stop other database writers;
3. create a fresh backup and hash;
4. run `verify-baseline`;
5. review the safe summary;
6. type the explicit database-name confirmation;
7. run `baseline-existing --to 13`;
8. run `status`;
9. run `verify`;
10. compare before/after application metadata;
11. compare before/after row counts and financial totals;
12. start the application using the runtime account;
13. run a read-only application health check.

## 12.6 Phase F — Apply later migrations separately

If versions `14+` exist:

1. keep the version-13 baseline evidence;
2. create another verified backup if required;
3. run `status`;
4. run the pending migrations explicitly;
5. verify each post-baseline change;
6. do not combine baseline registration and later migration execution into one hidden action.

---

# 13. Controlled baseline registration

## 13.1 Versions to record

The baseline operation records:

```text
1
2
3
4
5
6
7
8
9
10
11
12
13
```

Each row should use:

- the migration’s real description;
- the baseline operation’s UTC application timestamp;
- the normal FluentMigrator history shape.

## 13.2 Registration method

The baseline tool may use FluentMigrator’s supported version-loader/history APIs or a tightly controlled repository designed specifically for the migration-history table.

It must not:

- invoke migration `Up` methods;
- use a broad SQL file containing table creation;
- silently register whatever versions are currently present in the assembly;
- register versions above `13`;
- bypass schema verification;
- run from normal application startup.

## 13.3 Idempotency

A second baseline attempt must refuse safely.

Expected result:

```text
Database already contains migration history.
Current highest version: 13.
No baseline action was performed.
```

It must not insert duplicate history rows or reset timestamps.

## 13.4 Audit/log evidence

The migration tool logs:

- operation type;
- UTC start/end;
- database name;
- safe server identity;
- MariaDB version;
- migration assembly/application version;
- Git commit or build identifier where available;
- baseline target version;
- backup file name and SHA-256 supplied/verified;
- preflight result;
- inserted migration versions;
- verification result;
- failure reference.

It does not log credentials.

The application’s `audit_records` table does not need to be used for the pre-application migration tool. A separate migration log file is acceptable and avoids inventing an application user.

---

# 14. Post-baseline verification

## 14.1 Migration state

Expected:

```text
Highest applied migration: 13
Pending migration count:    0
schema_information version: 13
```

when no later migrations exist.

## 14.2 Expected schema change

Baseline registration may create:

```text
schema_migrations
```

It may update:

```text
schema_information.schema_version
schema_information.last_verified_utc
schema_information.date_updated_utc
```

No other application schema object or business row should change.

## 14.3 Metadata comparison

Compare application objects before and after, excluding:

```text
schema_migrations
```

Expected:

```text
Application table differences:      0
Application column differences:     0
Constraint differences:             0
Index differences:                  0
Foreign-key differences:            0
```

## 14.4 Data comparison

Capture before/after:

- table row counts;
- invoice net/VAT/gross totals;
- invoice payment totals;
- expense net/VAT/gross totals;
- account current-balance totals;
- balance-snapshot totals/counts;
- time-entry count and duration totals;
- task/customer/job counts.

Expected differences:

```text
0
```

except the explicitly approved migration-history and schema-information rows.

## 14.5 Runtime verification

After baseline:

- runtime account still connects;
- runtime account cannot alter schema;
- application startup compatibility check passes;
- logs contain no credentials;
- no migration executes on startup;
- a normal read-only health check succeeds.

---

# 15. Baseline failure and recovery

## 15.1 Preflight failure

If any baseline eligibility check fails:

- do not create or alter migration history;
- do not modify `schema_information`;
- report the mismatches;
- preserve the database;
- investigate and create an approved repair plan.

Do not use a `--force` switch to ignore structural differences.

## 15.2 Failure before history-table creation

No database change should exist.

Correct the problem and rerun verification.

## 15.3 Failure after history-table creation but before complete registration

Because MariaDB DDL can implicitly commit:

1. stop;
2. do not run application writes;
3. capture the partial migration-history state;
4. compare application schema/data;
5. restore the verified pre-baseline backup unless a reviewed recovery operation can remove only the incomplete history safely;
6. rerun the baseline process from the verified restored state.

Do not manually insert the remaining rows casually.

## 15.4 Verification failure after registration

If version rows were inserted but post-verification fails:

- treat the database as not safely baselined;
- stop application writes;
- preserve logs;
- restore the pre-baseline backup;
- identify whether the mismatch was in the schema, data or verification logic;
- correct the tool/migrations;
- retest on a disposable restored copy.

## 15.5 Failed later migration

For version `14+` failure:

- stop application writes;
- preserve migration logs;
- determine whether MariaDB committed partial DDL;
- restore the pre-migration backup when required;
- otherwise use a reviewed forward corrective migration;
- do not edit the released failed migration after it has been used outside a disposable database.

## 15.6 Backup retention

Retain the pre-baseline backup until:

- baseline verification passes;
- the application runs successfully;
- a subsequent fresh backup is completed and verified;
- the migration evidence has been committed/recorded.

---

# 16. Schema comparison rules

## 16.1 Include

Normalize and compare:

- table name;
- engine;
- table collation;
- column order;
- column name;
- column type;
- unsigned attribute;
- nullability;
- default;
- generated/extra attributes;
- primary key;
- unique constraints;
- foreign keys;
- update/delete behaviour;
- check clauses;
- index names;
- index uniqueness;
- index column order;
- index prefix length where used.

## 16.2 Exclude or normalize

Exclude/normalize:

- `schema_migrations`;
- table creation timestamps;
- internal table IDs;
- auto-increment next values;
- migration applied timestamps;
- whitespace and quoting differences in check clauses;
- non-semantic expression formatting;
- database name qualification where the schema is otherwise identical.

## 16.3 Seed comparison

Compare immutable identifiers and validity rather than all mutable values.

Examples:

- account type codes must exist and retain classification;
- required setting keys must exist;
- invoice sequence codes must exist;
- user-customised invoice prefixes may differ from original defaults;
- `next_number` may be higher than the seed;
- mutable setting values may differ if valid.

---

# 17. Test strategy

## 17.1 Unit tests

Test:

- baseline target is fixed at `13`;
- versions above `13` are never registered by baseline;
- a nonempty migration history blocks baseline;
- database-name confirmation must match exactly;
- fingerprint mismatch blocks baseline;
- missing backup/hash blocks baseline;
- duplicate baseline attempt is rejected;
- pending version `14+` is reported but not registered;
- sensitive connection values are redacted.

## 17.2 Integration tests

Using a dedicated MariaDB test environment:

1. apply migrations `1–13` to an empty database;
2. verify the 31-table application schema;
3. verify 116 checks or the current approved expected manifest;
4. compare with the bootstrap;
5. restore an unversioned baseline copy;
6. run baseline registration;
7. prove no initial `Up` migration was executed;
8. verify history contains exactly `1–13`;
9. verify application data did not change;
10. add a test migration `14`;
11. prove it remains pending after baseline;
12. apply `14` through explicit `migrate`;
13. verify status/history;
14. prove normal runtime credentials cannot run migration DDL;
15. prove tests refuse the normal development database.

## 17.3 Failure tests

Simulate:

- wrong database;
- partially matching schema;
- missing constraint;
- extra obsolete column;
- invalid seed code;
- existing migration-history row;
- backup path missing;
- hash mismatch;
- partial history registration;
- migration `14` failure;
- connection cancellation.

---

# 18. Environment safety guards

The migration tool must refuse to run destructive or baseline operations when:

- the database name is missing;
- the database name is the normal application database but explicit confirmation is absent;
- the test command targets `personal_business_manager`;
- the target contains unexpected production markers;
- the connection string is the runtime account without required migration privileges;
- the server identity differs from the approved target;
- the history state is unexpected;
- the schema fingerprint does not match;
- the required backup evidence is absent;
- a migration assembly version cannot be identified.

For the current local development database, the explicit baseline confirmation should require text equivalent to:

```text
BASELINE personal_business_manager TO 13
```

---

# 19. Repository and evidence requirements

The following must be tracked in Git:

```text
docs/decisions/migration_baseline_strategy.md
docs/decisions/schema_review.md
docs/personal_business_management_application_schema.sql
docs/schema_review_live_amendments.sql
src/PersonalBusinessManager.Infrastructure/Database/Migrations/
tools/PersonalBusinessManager.DatabaseMigrator/
tests/
```

The current broad `.gitignore` rule for `docs/` must be corrected.

Do not commit:

- database passwords;
- connection strings;
- local backup archives;
- database dumps containing real data;
- unredacted logs;
- user-specific absolute paths as required configuration.

Evidence may record a backup filename, size and hash without committing the backup itself.

---

# 20. Implementation sequence after this decision

P1-07 approves the policy but does not itself install FluentMigrator.

The next implementation sequence is:

1. correct repository tracking for `docs/`;
2. replace the MariaDB root runtime account;
3. create the dedicated migrator project/tool;
4. add FluentMigrator packages and MariaDB/MySQL runner support;
5. configure the `schema_migrations` history table;
6. implement migrations `1–13`;
7. implement schema manifest/comparison;
8. test empty-database migration;
9. test baseline against a restored disposable copy;
10. baseline the real development database;
11. add meaningful repository and migration tests;
12. keep migrations out of WinForms startup.

This corresponds to the Phase 2 migration work.

---

# 21. P1-07 decision checklist

## Required policy decisions

- [x] The current approved manually created schema is the initial baseline.
- [x] The exact baseline version is `13`.
- [x] Existing databases must not replay migrations `1–13`.
- [x] Empty future databases must be buildable through migrations `1–13`.
- [x] FluentMigrator history will use a dedicated `schema_migrations` table.
- [x] `schema_information` does not replace migration history.
- [x] The migration-history table is introduced only through the explicit migration tool.
- [x] A verified backup is required before baseline registration.
- [x] Backup restore is tested before real baseline registration.
- [x] Baseline is tested on a disposable restored copy first.
- [x] Runtime and migration database accounts are separate where practical.
- [x] Released migrations are immutable.
- [x] Post-baseline schema changes always use new migrations.
- [x] Normal application startup never baselines or migrates.
- [x] Versions `14+` are applied separately after baseline.
- [x] Rollback and recovery procedures are documented.
- [x] Schema and data parity checks are documented.
- [x] Migration logs must exclude credentials.

## Approved migration order

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

## Evidence to be completed in Phase 2

- [ ] Commit this decision document.
- [x] Correct the `docs/` Git ignore policy.
- [x] Add FluentMigrator packages.
- [x] Add the dedicated migration tool.
- [x] Implement migrations `1–13`.
- [x] Prove an empty database can be built through migrations.
- [x] Compare migrated schema with the approved bootstrap.
- [ ] Implement and test `baseline-existing`.
- [ ] Restore the current database backup to a disposable database.
- [ ] Baseline the disposable copy without replaying `Up`.
- [ ] Compare schema and data before/after.
- [ ] Baseline the real development database.
- [x] Verify history and application schema version are `13`.
- [x] Verify no credentials appear in logs.

---

# 22. Final decision

```text
Migration framework:                         APPROVED — FluentMigrator
Initial migration order:                    APPROVED — versions 1 through 13
Exact baseline version:                     APPROVED — 13
Current approved schema as initial baseline:APPROVED
Empty-database strategy:                    APPLY migrations 1 through 13
Existing-database strategy:                 VERIFY, THEN REGISTER 1 through 13
Automatic startup migration:                PROHIBITED
Future schema changes:                      NEW MIGRATIONS ONLY
P1-07 documentation gate:                   PASS
Phase 2 initial migrations (P2-04):         COMPLETE
Existing-schema baseline (P2-05):           PENDING
```

The migration order and baseline policy are now formally approved.

---

## 23. Approval record

**Owner:** Charlie Cook  
**Approval date:** 29 July 2026  
**Status:** Approved Phase 1 migration decision

### Non-negotiable baseline rule

```text
Never mark the existing database as version 13 unless it first matches the approved version-13 schema and seed requirements.
```

### Non-negotiable startup rule

```text
The normal WinForms application must never silently baseline or execute schema migrations.
```
