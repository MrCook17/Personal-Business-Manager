# MariaDB Integration-Test Environment

**Project:** Personal Business Manager

**Applies to:** Local MariaDB integration tests

**Approved database:** `personal_business_manager_test`

**Runtime account:** `personal_business_test_app@localhost`

**Migration account:** `personal_business_test_migrator@localhost`

This environment is isolated from the normal
`personal_business_manager` development database. It must never contain real
user or business data.

## 1. Connection separation

Use two process-scoped variables:

| Variable | Account | Purpose |
|---|---|---|
| `PBM_TEST_CONNECTION_STRING` | `personal_business_test_app` | Integration-test reads and writes |
| `PBM_TEST_MIGRATION_CONNECTION_STRING` | `personal_business_test_migrator` | Explicit test reset and migrations |

Do not use `PBM_CONNECTION_STRING`, `PBM_MIGRATION_CONNECTION_STRING`, or
MariaDB `root` for test execution. Root or another MariaDB administrator is
needed only for the one-time account creation or credential rotation.

Passwords must be generated independently. Windows Credential Manager or
Windows DPAPI remains the preferred long-term storage. For local development,
the repository-root `.env` file may hold the connection strings because it is
ignored by Git; never commit that file, place passwords in command arguments,
or print them in logs. Keep only placeholders in the committed `.env.example`.

## 2. One-time database and account creation

Run the following manually as a local MariaDB administrator after replacing
both placeholders with different random passwords:

```sql
CREATE DATABASE `personal_business_manager_test`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

CREATE USER 'personal_business_test_app'@'localhost'
    IDENTIFIED BY '<RANDOM_RUNTIME_TEST_PASSWORD>';

CREATE USER 'personal_business_test_migrator'@'localhost'
    IDENTIFIED BY '<DIFFERENT_RANDOM_MIGRATION_TEST_PASSWORD>';

GRANT SELECT, INSERT, UPDATE, DELETE
    ON `personal_business_manager_test`.*
    TO 'personal_business_test_app'@'localhost';

GRANT SELECT, INSERT, UPDATE, DELETE, CREATE, ALTER, INDEX, DROP,
      REFERENCES, CREATE VIEW, SHOW VIEW, TRIGGER
    ON `personal_business_manager_test`.*
    TO 'personal_business_test_migrator'@'localhost';

FLUSH PRIVILEGES;
```

The runtime account deliberately has no schema or account-administration
privileges. The migration account has no global administrative privilege and
no `GRANT OPTION`; its schema privileges are restricted to the approved test
database.

## 3. Configure local development and tests

Copy `.env.example` to `.env`, retrieve each password from protected storage,
and replace the placeholders. The WinForms application, database migrator,
and integration tests load the repository-root file into the current process.
An explicitly set process value takes precedence; on Windows, `.env` replaces
a matching value merely inherited from a persistent user environment variable.

Use these values:

```text
PBM_TEST_CONNECTION_STRING=
Server=127.0.0.1;Port=3306;Database=personal_business_manager_test;User ID=personal_business_test_app;Password=<retrieved-runtime-password>;SslMode=None;Connection Timeout=5;Default Command Timeout=30

PBM_TEST_MIGRATION_CONNECTION_STRING=
Server=127.0.0.1;Port=3306;Database=personal_business_manager_test;User ID=personal_business_test_migrator;Password=<retrieved-migration-password>;SslMode=None;Allow User Variables=true;Connection Timeout=5;Default Command Timeout=30
```

`PBM_CONNECTION_STRING` may point to the same restricted test runtime account
for local UI development. Do not put `root` or another administrative account
in `.env`. Delete the local file when the credentials are no longer needed.

## 4. Reset through the migration runner

The reset is deliberately destructive to
`personal_business_manager_test`. Stop other tests using that database, then
run:

```powershell
dotnet run --project tools/PersonalBusinessManager.DatabaseMigrator -- `
  reset-test `
  --connection-env PBM_TEST_MIGRATION_CONNECTION_STRING `
  --confirm "RESET TEST DATABASE personal_business_manager_test"
```

The command:

1. validates the connection before opening it;
2. requires the exact database, local host, migration account, environment
   variable, and confirmation;
3. acquires a MariaDB advisory reset lock;
4. drops and recreates only the approved test database;
5. runs the normal FluentMigrator runner from version `1` through the latest
   migration;
6. verifies the schema fingerprint, seeds, relationships, constraints, row
   integrity, history, and schema-information version.

There is no force option.

## 5. Run tests

After a successful reset:

```powershell
dotnet test PersonalBusinessManager.slnx
```

MariaDB tests are discovered as skipped when
`PBM_TEST_CONNECTION_STRING` is absent. When it is present but unsafe, the
test fails before opening a repository connection.

Each test must use unique natural keys where appropriate and delete inserted
records in `finally` or roll back its own transaction. A post-test query must
find no test-owned rows.

## 6. Safety guard

The shared guard rejects:

- `personal_business_manager`;
- names without the `_test` marker;
- names containing `prod`, `production`, `live`, or `staging` tokens;
- any database other than `personal_business_manager_test`;
- remote hosts;
- MariaDB `root`;
- the runtime account in the migration variable;
- the migration account in the runtime variable;
- absent database or connection values.

`reset-test` additionally rejects `PBM_MIGRATION_CONNECTION_STRING`, an
inexact confirmation, and baseline-only command options.

## 7. Testcontainers decision

Testcontainers was considered. It is deferred because Docker or another
supported container runtime is not yet an approved project dependency, and
the current local MariaDB `10.4.32` compatibility target is already available.

Reconsider Testcontainers when the production-LTS compatibility environment
is established. A future container setup must retain equivalent target guards,
least-privilege accounts, migration-runner setup, deterministic cleanup, and
credential handling.

## 8. P2-07 execution evidence — 31 July 2026

- The unsafe reset proof targeted `personal_business_manager` through the test
  migration variable. The guard refused with exit code `2` before reset; the
  live database retained 18 settings and migration history `1` through `13`.
- The approved test database was reset through
  `personal_business_test_migrator@localhost` on MariaDB `10.4.32`.
- Migrations `1` through `13` applied to the recreated empty database.
- Verification passed with 31 application tables, 959 normalized schema
  records, 116 checks, the approved schema fingerprint
  `7a85fdf6b3c6bd5d4a2d5ba1f47c33af24f5a46714b89a07939b19a24fb79b6f`,
  and the approved 39-row seed data fingerprint
  `6e620fcec64f25cdc2a7638496fd697bee2a5fd4062837327ada4671566987cb`.
- The database-enabled suite passed 59 of 59 tests with no failures or skips:
  1 Core test and 58 integration tests.
- Runtime verification connected as
  `personal_business_test_app@localhost`, found 18 settings, zero P2-06 test
  rows, and exact history `1` through `13`.
- Grant inspection confirmed CRUD-only runtime access and database-scoped
  migration access without `GRANT OPTION` or global administration.
- Generated passwords were handed to the owner separately and do not appear
  in this document, source, logs retained by the repository, or Git.
