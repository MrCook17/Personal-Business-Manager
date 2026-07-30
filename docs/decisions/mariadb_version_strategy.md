# MariaDB Version Strategy

**Project:** Personal Business Manager  
**Decision:** P2-02 — Record the MariaDB development-version decision  
**Document status:** Approved development and production version policy  
**Decision date:** 29 July 2026  
**Owner:** Charlie Cook  
**Repository path:** `docs/decisions/mariadb_version_strategy.md`  
**Current development server:** MariaDB `10.4.32` supplied through XAMPP  
**Production server:** Maintained MariaDB Community Server LTS, installed outside XAMPP  
**Current production-validation candidate:** MariaDB `12.3` LTS, latest stable patch available at validation/deployment time  
**Minimum currently tested server version:** MariaDB `10.4.32`

---

# 1. Executive decision

MariaDB `10.4.32` is approved only as temporary, local development infrastructure.

It is not approved for:

- production;
- internet-facing access;
- long-term server hosting;
- storing the only copy of important business or personal-finance data;
- defining the permanent minimum production version;
- use as evidence that the application is production-ready.

The final production deployment will use a maintained MariaDB Long-Term Support release installed as a proper database service on a maintained operating system.

The production server will not use:

```text
XAMPP
```

The production database will not be created by copying XAMPP’s MariaDB data directory.

As of 29 July 2026, MariaDB `12.3` is the latest MariaDB Community Server LTS series. Its first generally available release was `12.3.2` on 29 May 2026. It is the current provisional production-validation target.

The exact production patch version is not frozen now. At deployment time, the project must select:

```text
the latest stable patch of the approved maintained LTS series
```

after application, migration, backup and restore testing.

If a newer LTS exists when production deployment begins, the target must be reviewed again rather than deploying an older version automatically.

---

# 2. Current official version context

## 2.1 MariaDB 10.4 status

MariaDB `10.4` reached Community maintenance end of life on:

```text
18 June 2024
```

MariaDB `10.4.32` was released on:

```text
13 November 2023
```

The final `10.4` maintenance release was later than `10.4.32`, but the entire `10.4` series is now unmaintained.

Consequences:

- no normal Community security fixes for the series;
- no normal Community bug-fix releases;
- it must not become the production baseline;
- compatibility success on `10.4.32` does not prove compatibility with the production LTS;
- development use must remain local and temporary.

## 2.2 Current LTS context

As of the decision date:

```text
MariaDB 12.3 is the latest LTS series.
First GA patch: 12.3.2
GA date: 29 May 2026
Community maintenance target: 29 May 2029
```

MariaDB’s current LTS policy generally provides Community LTS binaries for three years after GA for newer annual LTS releases.

Other maintained LTS series may remain available concurrently.

For this project:

- `12.3` is the provisional production-validation target;
- `11.4` may be retained as a fallback comparison target if a confirmed compatibility problem exists;
- an older LTS must not be selected merely because it is closer to `10.4`;
- a rolling, preview, alpha, beta or release-candidate series is not approved for production.

## 2.3 Important MariaDB 12.3 behaviour

MariaDB has documented a changed default involving InnoDB snapshot isolation in `12.3`.

Therefore production validation must include transaction and concurrency tests rather than assuming all transaction behaviour is identical to `10.4`.

This is particularly relevant to:

- invoice number allocation;
- stopping an active timer;
- balance-snapshot updates;
- account-application conversion;
- payment recording and reversal;
- optimistic concurrency;
- migration/baseline operations.

---

# 3. Approved environment roles

## 3.1 Local development environment

Approved current role:

```text
MariaDB 10.4.32 through XAMPP
Local development only
```

Purpose:

- continued application development;
- local schema work already verified against this server;
- temporary compatibility testing;
- learning and prototyping;
- non-production integration tests when protected by a dedicated test database.

Restrictions:

- bind only to localhost unless a deliberate, temporary private-network test is required;
- do not expose the service to the public internet;
- do not treat the XAMPP control panel as production operations tooling;
- do not use `root` as the normal application runtime identity;
- do not keep the only backup in the XAMPP folder;
- do not build production deployment instructions around XAMPP paths;
- do not rely on XAMPP-specific behaviour in application code.

## 3.2 Production-validation environment

Before production, create a separate environment using the selected LTS.

Preferred purposes:

- apply FluentMigrator migrations to an empty database;
- baseline/import a copy of existing development data;
- run all Core and MariaDB integration tests;
- validate connector behaviour;
- test backups and restores;
- test performance;
- test transaction behaviour;
- test least-privilege accounts;
- test deployment and recovery procedures.

This environment may be:

- a disposable Linux virtual machine;
- a separate physical server;
- a Raspberry Pi 5 with 64-bit Linux and SSD, if it meets the project’s production requirements;
- a local container or VM for repeatable tests;
- another isolated maintained host.

It must not use the live production database.

## 3.3 Production environment

Approved role:

```text
Maintained MariaDB Community Server LTS
Maintained 64-bit operating system
Dedicated MariaDB service
No XAMPP
```

Production must use:

- a supported LTS series;
- the latest tested stable patch within that series;
- a dedicated application database;
- separate runtime and migration/admin accounts;
- firewall restrictions;
- tested backups;
- monitored storage;
- controlled updates;
- restore procedures;
- documented server configuration.

---

# 4. Production version-selection policy

## 4.1 Provisional target

The current provisional target is:

```text
MariaDB 12.3 LTS
```

Use the latest stable `12.3.x` patch available when production validation begins.

Do not freeze production specifically to:

```text
12.3.2
```

because later patch releases may contain security and corrective fixes.

## 4.2 Re-evaluation date

Re-evaluate the target:

- when the production server is purchased or provisioned;
- before the first production-validation cycle;
- immediately before production installation;
- if a new MariaDB LTS series becomes generally available;
- if the selected operating system repository provides a materially different supported LTS;
- if application compatibility testing fails.

## 4.3 Required selection criteria

The final production series must:

1. be officially designated LTS;
2. be generally available, not a preview or release candidate;
3. still receive Community maintenance;
4. support the chosen production operating system and CPU architecture;
5. support the project’s MariaDB connector and FluentMigrator provider;
6. pass the approved schema and migration tests;
7. pass backup and restore tests;
8. pass transaction and concurrency tests;
9. provide sufficient maintenance life after go-live;
10. have no known project-blocking compatibility issue.

## 4.4 Remaining-support threshold

At first production go-live, prefer an LTS with at least:

```text
18 months of Community maintenance remaining
```

Target:

```text
24 months or more where practical
```

If the intended LTS has less than 18 months remaining:

- review the next LTS;
- document why the older series is still preferable;
- define an earlier upgrade date;
- do not proceed through inertia.

## 4.5 Patch-level rule

Production uses:

```text
the latest tested stable patch in the approved LTS series
```

The application documentation records the exact tested patch.

Example:

```text
Approved series: 12.3
Tested patch:    12.3.x
Production patch:12.3.x
```

A newer patch may be installed only after staging/disposable testing.

---

# 5. Development compatibility policy

## 5.1 Temporary minimum tested version

The current minimum tested server version is:

```text
MariaDB 10.4.32
```

This means only:

```text
the current schema and current tests have run on this temporary development server
```

It does not mean:

- `10.4.32` is supported for production;
- every future migration must support `10.4.32` forever;
- the application will publish `10.4.32` as a customer/server requirement;
- production-specific testing is unnecessary.

## 5.2 Temporary dual-version requirement

Until the local development environment is upgraded, database work should be tested against:

1. MariaDB `10.4.32`; and
2. the selected production LTS test instance.

This dual test prevents:

- accidental dependence on XAMPP behaviour;
- false confidence from an obsolete server;
- last-minute discovery of production-LTS incompatibilities.

## 5.3 SQL feature rule

During the temporary dual-version period:

- baseline migrations must continue to work on `10.4.32` where already approved;
- new SQL must not depend on a later-version-only feature without an explicit decision;
- the production LTS remains the final source of production compatibility;
- compatibility workarounds must not weaken constraints or data integrity.

If a useful post-10.4 feature is needed:

1. document the feature;
2. confirm it is supported by the production LTS;
3. decide whether the local development server must be upgraded;
4. update the minimum tested version;
5. add tests.

Do not add conditional SQL branches casually based only on server version.

## 5.4 Development upgrade point

The local development server should be upgraded or replaced before:

- final production testing;
- production data migration;
- relying on features unavailable in `10.4`;
- exposing the application beyond the local machine;
- declaring production database compatibility complete.

The XAMPP database may continue temporarily during early development because its scope is explicitly limited.

---

# 6. Production platform policy

## 6.1 No XAMPP production server

XAMPP is not the production database platform.

Do not deploy production using:

- the XAMPP MariaDB service;
- the XAMPP `mysql\data` directory;
- XAMPP’s root account;
- XAMPP control-panel startup as the service-management plan;
- production paths tied to a developer workstation installation.

## 6.2 Preferred server installation

Install MariaDB using:

- the official MariaDB repository/packages; or
- maintained operating-system packages that provide the approved LTS and support policy.

For a Linux server, use the native service manager, normally:

```text
systemd
```

Production configuration belongs in the operating system’s normal MariaDB configuration directories.

## 6.3 Preferred operating system

Use a maintained 64-bit operating system with security support.

For a Raspberry Pi 5 deployment, use:

- 64-bit ARM operating system;
- SSD storage rather than relying on a microSD card for the database;
- maintained distribution packages or official MariaDB packages;
- adequate cooling and stable power;
- monitored disk health;
- off-device backups.

The exact operating system is a later deployment decision.

## 6.4 Network exposure

The database should not be exposed directly to the public internet.

Preferred access:

- application and database on the same host; or
- private LAN/VPN connectivity;
- firewall allow-listing;
- TLS where database traffic crosses a network;
- no broad public database-port exposure.

Remote access must use dedicated accounts and secure transport.

---

# 7. Account and privilege policy

## 7.1 Runtime account

The application runtime account:

```text
personal_business_app
```

or equivalent should have only required data privileges on the application database.

Typical early privileges:

```text
SELECT
INSERT
UPDATE
DELETE
```

It must not have:

```text
ALL PRIVILEGES ON *.*
GRANT OPTION
CREATE USER
DROP USER
global administrative privileges
```

## 7.2 Migration account

A separate migration/admin account may have schema privileges limited to:

```text
personal_business_manager.*
```

Use it only through explicit migration/maintenance tooling.

## 7.3 Production account recreation

Do not copy the XAMPP `mysql` system database or existing root/user rows into production.

Create production accounts fresh.

Reasons:

- avoid transferring obsolete authentication configuration;
- avoid transferring development-only grants;
- avoid copying XAMPP root assumptions;
- keep production secrets separate;
- preserve least privilege.

## 7.4 Credential storage

Production credentials must use the approved protected-storage strategy.

Do not:

- store them in source control;
- place them in logs;
- copy them from development;
- include them in database dumps shared as project artifacts.

---

# 8. Schema and migration compatibility

## 8.1 Authoritative schema process

Production databases are created and upgraded through FluentMigrator.

For an empty production database:

```text
apply approved migrations from version 1 onward
```

For an existing development database moved into production:

```text
follow the approved baseline and data-migration process
```

Do not run the bootstrap SQL as an uncontrolled upgrade script.

## 8.2 Required target tests

Against the production LTS test instance, verify:

- all 31 approved application tables;
- migration-history table;
- expected columns and types;
- all approved checks;
- foreign keys;
- unique constraints;
- indexes;
- seed data;
- default values;
- lowercase `snake_case`;
- `record_id` primary keys;
- exact duration seconds;
- invoice and expense financial constraints;
- audit null-actor behaviour.

## 8.3 Check-constraint verification

Because the project relies on MariaDB `CHECK` constraints, tests must prove the target LTS enforces them.

At minimum test rejection of:

- unknown workflow code;
- Boolean value outside `0` and `1`;
- inconsistent completed timestamp;
- inconsistent invoice structure;
- invalid discount percentage;
- blank payment-reversal reason;
- inconsistent financial totals.

## 8.4 Application-level invariants

Retest invariants that cannot be expressed as simple checks:

- a credit note cannot reference itself;
- one primary customer contact;
- one default address per type;
- account scope restrictions;
- invoice time eligibility;
- transaction atomicity;
- optimistic concurrency.

---

# 9. Connector compatibility

## 9.1 Approved connector

The application uses:

```text
MySqlConnector
```

rather than relying on an XAMPP-specific client library.

The selected connector version must be tested against:

- development MariaDB `10.4.32`;
- the selected production LTS;
- the application’s .NET target;
- Dapper;
- FluentMigrator’s selected provider.

## 9.2 Test areas

Verify:

- connection creation;
- async commands;
- cancellation tokens;
- `DATETIME(6)`;
- unsigned integer mapping;
- `DECIMAL` precision;
- nullable values;
- transaction isolation;
- check-constraint exceptions;
- duplicate-key exceptions;
- command timeout;
- connection pooling;
- server-version reporting.

## 9.3 No version-string business logic

Do not scatter conditions such as:

```csharp
if (serverVersion.StartsWith("10.4"))
```

through repositories.

If version-specific behaviour is unavoidable:

- centralise capability detection;
- document it;
- test both paths;
- remove it when the obsolete development version is retired.

---

# 10. Upgrade and migration approach

## 10.1 Preferred approach

The move from the XAMPP development server to production is a controlled migration to a fresh server.

Preferred high-level approach:

1. provision a clean maintained server;
2. install the approved MariaDB LTS;
3. secure the service;
4. create fresh runtime and migration accounts;
5. create an empty application database;
6. apply FluentMigrator migrations;
7. export and import approved application data where required;
8. run `mariadb-upgrade` when the chosen upgrade method requires it;
9. verify schema and data;
10. run the application test suite;
11. complete backup/restore testing;
12. perform cutover only after acceptance.

## 10.2 Do not copy the data directory

Do not copy:

```text
C:\xampp\mysql\data
```

to the production server.

Do not perform an unsupported binary/data-directory move between:

- Windows XAMPP and Linux;
- MariaDB `10.4` and a newer major series;
- different CPU architectures;
- different InnoDB/system-table layouts.

## 10.3 Logical export/import

For moving existing application data, prefer a tested logical export/import or controlled application-data migration.

A logical migration must:

- include the application database data;
- preserve `record_id` values;
- preserve UTC timestamps;
- preserve decimal precision;
- preserve invoice and sequence state;
- exclude MariaDB system schemas;
- exclude development users/grants;
- be tested on a disposable production-LTS instance;
- reconcile row counts and financial totals.

## 10.4 Schema-first versus full-dump restore

Preferred:

```text
create the target schema through FluentMigrator,
then import application data
```

This keeps production schema creation aligned with the migration history.

A full logical dump containing table DDL may be used only in a rehearsed migration path where:

- it does not conflict with FluentMigrator;
- the resulting schema is compared with the approved migration output;
- migration history is registered correctly;
- system schemas are excluded;
- the process has been tested end to end.

## 10.5 Official upgrade path

Do not assume that any direct binary in-place upgrade from `10.4` to the chosen LTS is safe.

Before migration:

- read the official current upgrade-path documentation;
- review intervening major-release incompatibilities;
- review removed variables/options;
- test the exact source and target versions;
- run the documented upgrade utility where required.

MariaDB recommends running `mariadb-upgrade` after upgrading between major releases.

## 10.6 Downgrade policy

Do not plan to downgrade the upgraded production data directory in place.

Major-version downgrade is not the rollback strategy.

Rollback uses:

- the old untouched server/environment; or
- restoration of a verified pre-upgrade logical/physical backup into the old version.

---

# 11. Migration rehearsal

## 11.1 Required rehearsal

Before production cutover:

1. create a fresh backup/export from a copy of the development database;
2. calculate and record a SHA-256 hash;
3. create a disposable server running the target LTS;
4. create fresh users and database;
5. run the complete schema/migration path;
6. import data;
7. run upgrade utilities where required;
8. compare schema metadata;
9. compare row counts;
10. compare financial totals;
11. run application tests;
12. generate and open representative PDFs;
13. test backups;
14. restore the production-LTS backup to another disposable instance;
15. document duration and failures.

## 11.2 Required data comparisons

Compare source and target:

- row count for every application table;
- invoice net, VAT and gross totals;
- payment totals and reversal states;
- expense net, VAT and gross totals;
- account balances;
- balance-snapshot counts and sums;
- time-entry counts and duration totals;
- invoice sequence next values;
- job and invoice numbers;
- attachment counts, file sizes and hashes;
- audit-event counts.

Expected differences must be documented.

## 11.3 Cutover rehearsal result

Production cutover is not approved until the rehearsal is repeatable and does not rely on manual, undocumented fixes.

---

# 12. Production go-live gates

All gates must pass.

## 12.1 Version gate

- [ ] Selected series is officially LTS.
- [ ] Selected patch is stable.
- [ ] Community maintenance remains active.
- [ ] Remaining support meets the approved threshold.
- [ ] Selected OS/architecture is supported.

## 12.2 Schema gate

- [ ] Empty database builds through FluentMigrator.
- [ ] Existing-data migration succeeds.
- [ ] Schema matches the approved migration manifest.
- [ ] All constraints are enforced.
- [ ] Migration history is correct.
- [ ] No pending unexpected migration exists.

## 12.3 Application gate

- [ ] Solution builds.
- [ ] Core tests pass.
- [ ] Integration tests pass against the LTS.
- [ ] Repository read/write tests pass.
- [ ] Login and settings work.
- [ ] Timer transactions work.
- [ ] Invoice finalisation works.
- [ ] Account balance updates work.
- [ ] Backup status works.
- [ ] No server-version-specific failure remains.

## 12.4 Security gate

- [ ] XAMPP is not used.
- [ ] Root is not used by the application.
- [ ] Runtime privileges are least privilege.
- [ ] Migration account is separate.
- [ ] Database port is not publicly exposed.
- [ ] Credentials are protected.
- [ ] Logs contain no credentials.
- [ ] Firewall rules are reviewed.

## 12.5 Backup gate

- [ ] Pre-cutover backup exists.
- [ ] Hash is recorded.
- [ ] Restore test succeeds.
- [ ] Off-device backup exists.
- [ ] Attachment files are included.
- [ ] Generated documents are included where required.
- [ ] Restore instructions are documented.

## 12.6 Operational gate

- [ ] Disk-space monitoring exists.
- [ ] Database service starts automatically.
- [ ] Server time is correct.
- [ ] UTC database/session policy is verified.
- [ ] Patch process is documented.
- [ ] Recovery contact/process is known.
- [ ] Old development server is not mistaken for production.

---

# 13. Patch and maintenance policy

## 13.1 Development server

MariaDB `10.4.32` remains frozen only for temporary compatibility.

Do not invest in treating it as a maintained production platform.

Continue:

- local firewall protection;
- development backups;
- least-privilege runtime account;
- separate test database;
- no public exposure.

## 13.2 Production LTS patches

For the selected LTS:

1. review new stable patch release notes;
2. back up production;
3. test the patch in staging/disposable environment;
4. run application and integration tests;
5. verify backup/restore tooling;
6. schedule maintenance;
7. install the patch;
8. run `mariadb-upgrade` if required;
9. verify schema/application health;
10. retain rollback backup.

## 13.3 Security updates

Critical security fixes should be assessed immediately.

Target:

```text
critical/exploitable issue: test and patch as soon as practical
normal corrective LTS patch: test and apply within 30 days where practical
```

A faster schedule may be required where the server is network-accessible or the issue affects the current configuration.

## 13.4 LTS replacement planning

Begin planning the next major LTS migration no later than:

```text
12 months before Community maintenance ends
```

Target completion:

```text
at least 6 months before end of maintenance
```

Do not wait until the final month.

---

# 14. Backup and rollback strategy

## 14.1 Before every major-version migration

Create:

- full logical database export;
- server/configuration record;
- attachment/document backup;
- SHA-256 hashes;
- source version record;
- row-count and financial-total report.

Restore the backup before trusting it.

## 14.2 Preserve old environment

For the first production migration, retain the old development/source environment unchanged until:

- target validation passes;
- cutover passes;
- a post-cutover backup is created;
- a post-cutover restore test passes;
- the application operates correctly for the agreed observation period.

## 14.3 Rollback trigger examples

Rollback may be required for:

- missing data;
- financial-total mismatch;
- constraint differences;
- failed application transactions;
- unacceptable performance;
- connector incompatibility;
- backup failure;
- data corruption;
- security misconfiguration.

## 14.4 Rollback method

Rollback means:

1. stop writes to the new target;
2. preserve target logs/evidence;
3. restore service using the old environment or old-version backup;
4. reconcile any writes made after cutover;
5. investigate on a copy;
6. repeat the migration rehearsal.

Do not attempt an unsupported in-place major-version downgrade.

---

# 15. Compatibility test matrix

## 15.1 Temporary matrix

| Environment | Version | Purpose | Production approved |
|---|---|---|---|
| Local XAMPP development | `10.4.32` | Temporary development and minimum compatibility | No |
| Disposable production-LTS instance | Latest tested `12.3.x` | Migration, integration and compatibility testing | No |
| Production server | Latest approved stable LTS patch | Live application | Yes, after all gates |

## 15.2 Required database tests

Run against both temporary development and the production LTS where applicable:

- health check;
- migration status;
- Dapper repository read;
- Dapper repository write;
- cancellation;
- unique constraint;
- foreign-key restriction;
- check constraint;
- optimistic concurrency;
- timer stop transaction;
- invoice sequence locking;
- payment recording/reversal;
- balance-snapshot transaction;
- backup metadata write;
- UTF-8 text;
- date/time precision;
- decimal precision.

## 15.3 Target-LTS-only tests

Test:

- transaction isolation/default differences;
- query plans for core list queries;
- connector/server authentication;
- migration tooling;
- backup utility versions;
- server service configuration;
- filesystem permissions;
- production architecture.

---

# 16. Performance expectations

The version migration is not complete merely because the schema imports.

Validate representative operations:

- application startup health check;
- login lookup;
- customer search;
- job list filtering;
- time-entry list;
- invoice list;
- invoice finalisation;
- dashboard summaries;
- personal-account summaries;
- audit-history paging;
- backup creation.

Use representative data volumes.

Investigate:

- query plans;
- missing/redundant indexes;
- lock waits;
- connection-pool behaviour;
- disk latency;
- long-running commands.

Do not use server-version change as justification for premature indexing without measurements.

---

# 17. Production configuration principles

The exact configuration belongs to deployment documentation, but the following are mandatory principles:

- use UTC server/database session conventions;
- use InnoDB;
- use `utf8mb4`;
- configure an explicit backup destination;
- keep database files on reliable SSD storage;
- monitor free disk space;
- configure sensible connection limits;
- keep binary logging only where required by recovery/replication policy;
- review log retention;
- protect configuration-file permissions;
- do not disable integrity checks to make imports pass;
- do not enable experimental plugins without need;
- document every non-default production setting.

---

# 18. Documentation and evidence

Record for each environment:

```text
MariaDB series
Exact patch
Operating system
CPU architecture
Installation source
Connector version
FluentMigrator version/provider
Schema version
Migration status
Runtime account identity
Backup utility/version
Last compatibility test date
Last restore-test date
```

Safe version diagnostics may be logged.

Do not log credentials or complete connection strings.

---

# 19. Repository requirements

Commit:

```text
docs/decisions/mariadb_version_strategy.md
docs/decisions/migration_baseline_strategy.md
docs/decisions/schema_review.md
deployment documentation
migration scripts/code
test configuration instructions
```

Do not commit:

- database dumps containing real data;
- passwords;
- production connection strings;
- private keys;
- unredacted logs;
- local XAMPP configuration containing secrets.

The broad `.gitignore` rule that currently excludes `docs/` must be corrected so decision evidence is tracked normally.

---

# 20. Decision boundaries

## 20.1 Approved now

- MariaDB `10.4.32` remains temporary local development infrastructure.
- XAMPP is prohibited for production.
- Production uses a maintained MariaDB LTS.
- MariaDB `12.3` is the current provisional validation target.
- The exact stable patch is selected at deployment time.
- The target is re-evaluated if a newer LTS exists.
- Production uses a fresh server installation.
- The migration is rehearsed and backed up.
- Major-version downgrade is not the rollback strategy.

## 20.2 Not decided by this document

- exact production hardware;
- exact Linux distribution;
- whether production runs on Raspberry Pi 5 or another host;
- exact deployment date;
- exact `12.3.x` patch;
- VPN/provider choice;
- replication/high-availability architecture;
- enterprise support subscription.

These decisions do not block P2-02.

---

# 21. P2-02 verification checklist

## Required decisions

- [x] MariaDB `10.4.32` is temporary development infrastructure only.
- [x] The current development server is XAMPP-based.
- [x] XAMPP is not approved for production.
- [x] MariaDB `10.4` is recorded as unmaintained.
- [x] The minimum currently tested server version is recorded.
- [x] Production will use a maintained LTS.
- [x] MariaDB `12.3` is the current provisional production-validation target.
- [x] The exact production patch will be selected after testing.
- [x] The target will be re-evaluated at deployment time.
- [x] Production installation will be separate from XAMPP.
- [x] Backups and restores are required before migration.
- [x] A disposable target migration rehearsal is required.
- [x] Separate runtime and migration accounts are required.
- [x] The database will not be exposed directly to the public internet.
- [x] Major-version downgrade is not the rollback plan.
- [x] No production deployment may depend indefinitely on the old XAMPP server.

## Evidence required before production

- [ ] Provision the selected maintained LTS test environment.
- [ ] Record its exact patch and operating system.
- [ ] Apply all migrations to an empty target.
- [ ] Import a disposable copy of existing data.
- [ ] Compare schema and data.
- [ ] Run Core and integration tests.
- [ ] Test transaction/isolation behaviour.
- [ ] Test backup and restore.
- [ ] Test performance.
- [ ] Confirm least-privilege accounts.
- [ ] Complete production go-live gates.
- [ ] Commit this document.

---

# 22. Final decision

```text
Current local development version:       MariaDB 10.4.32
Development server platform:             XAMPP
Development use approved:                YES — temporary and local only
Production use of 10.4.32:               PROHIBITED
Production use of XAMPP:                 PROHIBITED
Current production-validation candidate: MariaDB 12.3 LTS
Exact production patch:                  SELECT AFTER TESTING
Production installation:                 FRESH MAINTAINED SERVER
Migration rehearsal:                     REQUIRED
Backup and restore test:                 REQUIRED
P2-02 documentation gate:                PASS
Production compatibility gate:           PENDING
```

---

## 23. Approval record

**Owner:** Charlie Cook  
**Approval date:** 29 July 2026  
**Status:** Approved development and production version strategy

### Non-negotiable development rule

```text
MariaDB 10.4.32 may remain for local development, but it must never become the production baseline.
```

### Non-negotiable production rule

```text
Production must use a currently maintained MariaDB LTS installed outside XAMPP and verified through migrations, tests, backup and restore.
```

---

# 24. Official source record

Official MariaDB information consulted on 29 July 2026:

1. **MariaDB Foundation — MariaDB Server 12.3 LTS Released**
   - identifies `12.3` as the latest LTS;
   - records the first GA release as `12.3.2`;
   - records the GA announcement date as 29 May 2026;
   - advises staging, backup and compatibility testing;
   - identifies the changed InnoDB snapshot-isolation default.

2. **MariaDB Foundation — About MariaDB Server / Maintenance Policy**
   - records Community maintenance policy;
   - records MariaDB `10.4` Community end of maintenance as 18 June 2024;
   - records compatibility expectations and major-version change risk.

3. **MariaDB Community Server Release Notes**
   - identifies `12.3` as the latest long-term stable series as of the decision date.

4. **MariaDB Server Upgrade Documentation**
   - requires review of official major-version upgrade paths;
   - recommends backup and compatibility testing;
   - documents `mariadb-upgrade` for major-version upgrades.

5. **MariaDB Downgrade Documentation**
   - states that major-version downgrade is not the supported rollback approach.
