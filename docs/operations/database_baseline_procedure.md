# Existing Database Baseline Procedure

**Project:** Personal Business Manager

**Applies to:** Existing approved version-13 MariaDB databases

**Baseline target:** `13`

**Tool:** `tools/PersonalBusinessManager.DatabaseMigrator`

**Policy source:** `docs/decisions/migration_baseline_strategy.md`

This procedure registers an existing approved schema with FluentMigrator
without executing migrations `1` through `13`.

It must never be run automatically by the WinForms application.

## 1. Preconditions

Before any baseline write:

1. Stop the WinForms application and other database writers.
2. Confirm migrations `1` through `13`, the manifest, and the migration tool
   are committed.
3. Confirm `git status --short` is empty.
4. Build the solution and run all tests.
5. Use the dedicated migration connection environment variable. Never put a
   connection string or password on the command line.
6. Confirm the target using read-only `status`.
7. Run read-only `verify-baseline`; every check must pass.

Do not continue after a warning, fingerprint mismatch, missing seed, integrity
failure, or unexpected migration-history row.

There is no `--force` override.

## 2. Create and verify a backup

Create a full logical backup containing schema, data, routines, triggers, and
events. Use a protected client option file, secure credential facility, or
interactive password input. Never include a password in process arguments.

The backup filename must include a UTC timestamp, for example:

```text
personal_business_manager_pre_baseline_YYYYMMDD_HHMMSSZ.sql
```

After the dump process exits successfully:

1. record the filename;
2. record the byte size;
3. calculate SHA-256;
4. retain the closed file outside the XAMPP installation;
5. do not commit the dump to Git.

## 3. Restore-test the exact backup

Create a uniquely named disposable database using `utf8mb4_unicode_ci`.
Restore the exact hashed backup into it.

Against the disposable restored copy:

1. compare all 31 per-table row counts with the source;
2. compare invoice, payment, expense, account-balance, balance-snapshot, and
   time-duration aggregates;
3. run `status`;
4. run `verify-baseline`;
5. run `baseline-existing --to 13` using the exact backup path and hash;
6. run `status` again;
7. run `verify`;
8. confirm history contains exactly versions `1` through `13`;
9. confirm the application-schema fingerprint is unchanged;
10. confirm the application-data summary fingerprint is unchanged;
11. repeat `baseline-existing` and confirm it refuses;
12. run `migrate` separately and confirm either no pending migrations or only
    deliberately reviewed versions above `13`.

Drop only the verified disposable database after the evidence has been
recorded.

## 4. Baseline the real development database

Only after the restored-copy rehearsal passes:

1. keep the application and other writers stopped;
2. create a fresh pre-baseline backup and SHA-256;
3. run `verify-baseline` against `personal_business_manager`;
4. review the safe server, database, account, schema fingerprint, and data
   fingerprint;
5. run:

```powershell
dotnet run --project tools/PersonalBusinessManager.DatabaseMigrator -- `
  baseline-existing `
  --connection-env PBM_MIGRATION_CONNECTION_STRING `
  --to 13 `
  --backup-path "<verified-backup.sql>" `
  --backup-sha256 "<64-character-sha256>" `
  --confirm "BASELINE personal_business_manager TO 13"
```

6. run `status`;
7. run `verify`;
8. run `migrate` separately;
9. confirm versions and application schema version are `13` when no later
   migration exists;
10. confirm before/after data summary fingerprints match;
11. perform a read-only application database health check;
12. retain the pre-baseline backup and evidence.

## 5. Expected baseline-only changes

The operation may:

- create `schema_migrations`;
- insert migration-history versions `1` through `13`;
- set `schema_information.schema_version` to `13`;
- update `schema_information.last_verified_utc`;
- update `schema_information.date_updated_utc`.

It must not:

- execute any migration `Up()` method;
- create or alter an application table;
- modify a business row or mutable setting;
- reduce an invoice sequence;
- register version `14` or above;
- log a credential or connection string.

## 6. Failure and recovery

If preflight fails, no write is permitted.

If a failure occurs after history-table creation:

1. stop all writes;
2. retain the failure log;
3. inspect `schema_migrations`;
4. do not manually add missing history rows;
5. restore the verified pre-baseline backup unless a reviewed recovery plan
   proves that only incomplete migration metadata can be removed safely;
6. correct the tool or database mismatch;
7. repeat the restored-copy rehearsal before trying the real database again.

Keep the pre-baseline backup until baseline verification, application health
verification, and a later fresh backup have all succeeded.

## 7. Evidence record

Record without credentials:

- Git commit/build identifier;
- safe target server and database names;
- MariaDB version and account identity;
- backup filename, size, and SHA-256;
- restore-test database name and result;
- preflight fingerprint and result;
- inserted versions;
- before/after application-schema fingerprint;
- before/after application-data summary fingerprint;
- row-count and financial-aggregate comparison;
- final `status`, `verify`, and separate `migrate` results;
- baseline log filename and SHA-256.
