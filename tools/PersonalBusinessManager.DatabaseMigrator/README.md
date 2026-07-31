# Personal Business Manager database migrator

This dedicated console tool owns explicit database migration operations. The
normal WinForms application does not reference or invoke it.

## Connection

Store the migration/admin connection in one of the approved environment
variables:

```text
PBM_MIGRATION_CONNECTION_STRING
PBM_TEST_MIGRATION_CONNECTION_STRING
```

The selected variable must be named explicitly with `--connection-env`. The
tool deliberately does not accept a raw connection string on the command line,
so a password is not written to shell history.

Use an account with schema privileges limited to the selected application or
test database. Do not reuse the normal `PBM_CONNECTION_STRING` runtime account.

## Read-only status

```powershell
dotnet run --project tools/PersonalBusinessManager.DatabaseMigrator -- `
  status `
  --connection-env PBM_MIGRATION_CONNECTION_STRING
```

`status` reads safe target identity, application-table count,
`schema_migrations`, `schema_information`, applied versions, and migrations
pending in the Infrastructure assembly. It does not create or alter a table.

## Verify an existing baseline

```powershell
dotnet run --project tools/PersonalBusinessManager.DatabaseMigrator -- `
  verify-baseline `
  --connection-env PBM_MIGRATION_CONNECTION_STRING
```

`verify-baseline` is read-only. It compares normalized table, column,
constraint, check-clause, foreign-key-rule, and ordered-index metadata with the
approved version-13 fingerprint. It also verifies required seeds, the
schema-information singleton, all current foreign-key relationships, and every
enforced check expression against existing rows.

## Register an approved existing schema

Create and restore-test a full backup first. Calculate the SHA-256 after the
backup file has closed, then run:

```powershell
dotnet run --project tools/PersonalBusinessManager.DatabaseMigrator -- `
  baseline-existing `
  --connection-env PBM_MIGRATION_CONNECTION_STRING `
  --to 13 `
  --backup-path "<verified-backup.sql>" `
  --backup-sha256 "<64-character-sha256>" `
  --confirm "BASELINE personal_business_manager TO 13"
```

The tool hashes the supplied backup itself, repeats baseline eligibility
verification, requires an exact database-name confirmation, and registers only
versions `1` through `13`. It never invokes their `Up()` methods. Before/after
application row counts and approved financial aggregates must match exactly.
A second baseline attempt refuses safely.

## Verify the current migrated state

```powershell
dotnet run --project tools/PersonalBusinessManager.DatabaseMigrator -- `
  verify `
  --connection-env PBM_MIGRATION_CONNECTION_STRING
```

`verify` requires exact versions `1` through `13`,
`schema_information.schema_version = 13`, the approved schema fingerprint,
valid seeds, and valid existing data.

## Apply pending migrations

```powershell
dotnet run --project tools/PersonalBusinessManager.DatabaseMigrator -- `
  migrate `
  --connection-env PBM_MIGRATION_CONNECTION_STRING `
  --confirm "MIGRATE personal_business_manager"
```

The confirmation must match the database selected by the connection exactly.
The command refuses an existing schema with no applied migration history. Such
a database must use the guarded `baseline-existing` workflow; initial
migrations are never replayed over it. After baseline registration, run
`migrate` separately so any future versions above `13` remain a distinct,
explicit operation.

The tool contains the approved migrations 1-13 implemented under P2-04.
Existing schemas use the guarded baseline verification and registration
workflow added under P2-05.

The table- and seed-creating migrations deliberately reject `Down()` because
those operations would destroy data or mutable configuration. Migration 12
can be reversed safely and drops only the secondary indexes it created.
