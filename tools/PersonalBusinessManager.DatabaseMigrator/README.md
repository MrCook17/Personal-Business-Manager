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

## Apply pending migrations

```powershell
dotnet run --project tools/PersonalBusinessManager.DatabaseMigrator -- `
  migrate `
  --connection-env PBM_MIGRATION_CONNECTION_STRING `
  --confirm "MIGRATE personal_business_manager"
```

The confirmation must match the database selected by the connection exactly.
The command refuses an existing schema with no applied migration history. Such
a database must later use the guarded `baseline-existing` workflow defined by
P2-05; initial migrations are never replayed over it.

The tool contains the approved migrations 1-13 implemented under P2-04.
Existing schemas still require the separately guarded baseline verification
and registration workflow added under P2-05.

The table- and seed-creating migrations deliberately reject `Down()` because
those operations would destroy data or mutable configuration. Migration 12
can be reversed safely and drops only the secondary indexes it created.
