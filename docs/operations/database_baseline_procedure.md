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

## 8. P2-05 execution record — 31 July 2026

The first controlled use of this procedure completed successfully against
MariaDB `10.4.32` on the local development server.

### Committed tool

- Tooling commit: `d3853ff`
- Executed build:
  `1.0.0+d3853ff99d7c720f51ad0351f6d0fbf0422e7a31`
- Repository state before execution: clean
- Approved baseline: exactly versions `1` through `13`

### Restore-test evidence

- Backup:
  `Backups/P2-05/personal_business_manager_restore_test_20260731_093351Z.sql`
- Size: 70,678 bytes
- SHA-256:
  `ea29875061c80a98483a8da7249565d7776cf977cf782eadb40939bffb0c8f0f`
- Final committed-build restored copy:
  `pbm_p205_committed_copy_20260731`
- Restore result: success
- Baseline result: versions `1` through `13` registered; no `Up()` method
  executed
- Repeat result: refused safely because migration history was no longer empty
- Later migration result: no pending migration above version `13`
- Baseline log: `committed_copy_baseline_d3853ff.log`
- Baseline log SHA-256:
  `15a0b86c484b4d129ad70b9c0cb1633f161f49c9705e6d5056e5db22e61497dc`
- Post-verification log: `committed_copy_postverify_d3853ff.log`
- Post-verification log SHA-256:
  `d66272f679043328bd4e307795eeb1c10f61397c6f403c2be141ca081ba05a9e`

### Schema and data comparison

- Before/after schema fingerprint:
  `7a85fdf6b3c6bd5d4a2d5ba1f47c33af24f5a46714b89a07939b19a24fb79b6f`
- Normalized schema records: 959
- Application tables: 31
- Check constraints evaluated: 116
- Before/after data fingerprint:
  `6e620fcec64f25cdc2a7638496fd697bee2a5fd4062837327ada4671566987cb`
- Row counts compared: all 31 tables
- Total application rows before/after: 39
- Financial aggregates compared: all 14 approved totals
- Financial totals before/after: zero
- Unexpected application schema or data change: none

### Real development baseline

- Database: `personal_business_manager`
- Fresh backup:
  `Backups/P2-05/personal_business_manager_pre_baseline_20260731_093647Z.sql`
- Size: 70,678 bytes
- SHA-256:
  `52a52b91f2b7d6d88da32d0310100e02a1d3d110b2e8b0ffc80bacaf147258eb`
- Preflight: passed
- Registered history: exactly `1,2,3,4,5,6,7,8,9,10,11,12,13`
- `schema_information.schema_version`: `13`
- Post-baseline `status`: passed
- Post-baseline read-only `verify`: passed
- Separate `migrate`: passed; no pending migration
- Baseline log: `live_baseline_d3853ff_20260731_093647Z.log`
- Baseline log SHA-256:
  `0fc1847aadfdbfec883d961a1e46ceea57cf31dfca4ea67fc13898c071c66647`
- Post-verification log:
  `live_postverify_d3853ff_20260731_093647Z.log`
- Post-verification log SHA-256:
  `e8ba5b95982f054e19f545da7de82a66ad8733f4ad79e9646b05555adc5a067b`
- Credential or connection-string content in evidence logs: none

The disposable reference, first restore, and committed-build restore databases
were dropped after verification. Both logical backups and all logs remain in
the ignored `Backups/P2-05` directory. They contain development data and must
be protected, retained until the next verified backup, and never committed.

The supplied runtime and migrator passwords had already been disclosed and
were therefore not used or stored by this execution. Rotate both credentials
before the next account-specific health or migration operation, then store
them using the approved Windows credential mechanism.
