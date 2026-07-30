/*
    Personal Business Management Application
    Initial MariaDB schema bootstrap

    Source of truth:
      personal_business_management_application_final_plan.md
      Version 1.0, dated 27 July 2026

    Target:
      MariaDB 11.8 LTS preferred
      Compatible with MariaDB 10.4+ for local development where practical

    Important:
      - This script is non-destructive: it does not drop the database or tables.
      - It is intended for a new/empty database.
      - Future production changes should be applied through versioned FluentMigrator migrations.
      - No default administrator account is created because passwords must be hashed by the application.
*/

SET @previous_sql_mode := @@SESSION.sql_mode;
SET @previous_time_zone := @@SESSION.time_zone;
SET @previous_foreign_key_checks := @@SESSION.foreign_key_checks;

SET SESSION sql_mode = 'STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';
SET SESSION time_zone = '+00:00';
SET SESSION foreign_key_checks = 1;
SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE DATABASE IF NOT EXISTS `personal_business_manager`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE `personal_business_manager`;

/* ========================================================================== */
/* 1. SECURITY AND SYSTEM TABLES                                               */
/* ========================================================================== */

CREATE TABLE IF NOT EXISTS `users` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `username` VARCHAR(100) NOT NULL,
    `username_normalised` VARCHAR(100) NOT NULL,
    `display_name` VARCHAR(150) NOT NULL,
    `password_hash` VARCHAR(500) NOT NULL,
    `role_code` VARCHAR(50) NOT NULL DEFAULT 'administrator',
    `is_active` TINYINT(1) NOT NULL DEFAULT 1,
    `failed_login_count` INT UNSIGNED NOT NULL DEFAULT 0,
    `locked_until_utc` DATETIME(6) NULL,
    `password_changed_utc` DATETIME(6) NOT NULL,
    `last_login_utc` DATETIME(6) NULL,
    `date_created_utc` DATETIME(6) NOT NULL,
    `date_updated_utc` DATETIME(6) NOT NULL,
    `version_no` INT UNSIGNED NOT NULL DEFAULT 1,
    CONSTRAINT `pk_users` PRIMARY KEY (`record_id`),
    CONSTRAINT `uq_users_username_normalised` UNIQUE (`username_normalised`),
    CONSTRAINT `chk_users_role_code`
        CHECK (`role_code` IN ('administrator')),
    CONSTRAINT `chk_users_is_active` CHECK (`is_active` IN (0, 1)),
    CONSTRAINT `chk_users_password_changed`
        CHECK (`password_changed_utc` >= `date_created_utc`),
    CONSTRAINT `chk_users_failed_login_count` CHECK (`failed_login_count` >= 0),
    CONSTRAINT `chk_users_version_no` CHECK (`version_no` >= 1)
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Application users and local authentication state.';

CREATE TABLE IF NOT EXISTS `password_recovery_codes` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `user_id` BIGINT UNSIGNED NOT NULL,
    `recovery_code_hash` VARCHAR(500) NOT NULL,
    `created_utc` DATETIME(6) NOT NULL,
    `used_utc` DATETIME(6) NULL,
    `expires_utc` DATETIME(6) NULL,
    CONSTRAINT `pk_password_recovery_codes` PRIMARY KEY (`record_id`),
    CONSTRAINT `uq_password_recovery_codes_hash` UNIQUE (`recovery_code_hash`),
    CONSTRAINT `fk_password_recovery_codes_user_id`
        FOREIGN KEY (`user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE CASCADE,
    CONSTRAINT `chk_password_recovery_codes_expiry`
        CHECK (`expires_utc` IS NULL OR `expires_utc` > `created_utc`),
    CONSTRAINT `chk_password_recovery_codes_used`
        CHECK (`used_utc` IS NULL OR `used_utc` >= `created_utc`)
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Hashed one-time local administrator recovery codes.';

CREATE INDEX IF NOT EXISTS `idx_password_recovery_codes_user_created`
    ON `password_recovery_codes` (`user_id`, `created_utc`);

CREATE TABLE IF NOT EXISTS `application_settings` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `setting_key` VARCHAR(150) NOT NULL,
    `setting_value` LONGTEXT NULL,
    `value_type_code` VARCHAR(50) NOT NULL,
    `is_sensitive` TINYINT(1) NOT NULL DEFAULT 0,
    `date_updated_utc` DATETIME(6) NOT NULL,
    `updated_by_user_id` BIGINT UNSIGNED NULL,
    CONSTRAINT `pk_application_settings` PRIMARY KEY (`record_id`),
    CONSTRAINT `uq_application_settings_setting_key` UNIQUE (`setting_key`),
    CONSTRAINT `fk_application_settings_updated_by_user_id`
        FOREIGN KEY (`updated_by_user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE SET NULL,
    CONSTRAINT `chk_application_settings_value_type_code`
        CHECK (`value_type_code` IN ('string', 'integer', 'decimal', 'boolean', 'date', 'datetime', 'json')),
    CONSTRAINT `chk_application_settings_is_sensitive`
        CHECK (`is_sensitive` IN (0, 1))
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Configurable application and business settings. Do not store database credentials here.';

CREATE TABLE IF NOT EXISTS `schema_information` (
    `record_id` TINYINT UNSIGNED NOT NULL,
    `application_version` VARCHAR(30) NOT NULL,
    `minimum_supported_application_version` VARCHAR(30) NOT NULL,
    `schema_version` INT UNSIGNED NOT NULL,
    `last_verified_utc` DATETIME(6) NULL,
    `date_updated_utc` DATETIME(6) NOT NULL,
    CONSTRAINT `pk_schema_information` PRIMARY KEY (`record_id`),
    CONSTRAINT `chk_schema_information_singleton` CHECK (`record_id` = 1),
    CONSTRAINT `chk_schema_information_schema_version` CHECK (`schema_version` >= 1)
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Application-level schema compatibility information. FluentMigrator maintains its own version table.';

CREATE TABLE IF NOT EXISTS `audit_records` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `user_id` BIGINT UNSIGNED NULL,
    `entity_type_code` VARCHAR(100) NOT NULL,
    `entity_record_id` BIGINT UNSIGNED NULL,
    `action_code` VARCHAR(100) NOT NULL,
    `action_reason` TEXT NULL,
    `occurred_utc` DATETIME(6) NOT NULL,
    `old_values_json` LONGTEXT NULL,
    `new_values_json` LONGTEXT NULL,
    `correlation_id` CHAR(36) NULL,
    CONSTRAINT `pk_audit_records` PRIMARY KEY (`record_id`),
    CONSTRAINT `fk_audit_records_user_id`
        FOREIGN KEY (`user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `chk_audit_records_old_values_json`
        CHECK (`old_values_json` IS NULL OR JSON_VALID(`old_values_json`)),
    CONSTRAINT `chk_audit_records_new_values_json`
        CHECK (`new_values_json` IS NULL OR JSON_VALID(`new_values_json`))
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Append-only audit history for important changes and workflows.';

CREATE INDEX IF NOT EXISTS `idx_audit_records_entity_occurred`
    ON `audit_records` (`entity_type_code`, `entity_record_id`, `occurred_utc`);
CREATE INDEX IF NOT EXISTS `idx_audit_records_user_occurred`
    ON `audit_records` (`user_id`, `occurred_utc`);
CREATE INDEX IF NOT EXISTS `idx_audit_records_correlation_id`
    ON `audit_records` (`correlation_id`);

CREATE TABLE IF NOT EXISTS `backup_records` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `backup_type_code` VARCHAR(50) NOT NULL,
    `status_code` VARCHAR(50) NOT NULL,
    `started_utc` DATETIME(6) NOT NULL,
    `completed_utc` DATETIME(6) NULL,
    `verified_utc` DATETIME(6) NULL,
    `restored_utc` DATETIME(6) NULL,
    `archive_relative_path` VARCHAR(1024) NULL,
    `archive_size_bytes` BIGINT UNSIGNED NULL,
    `sha256_hash` CHAR(64) NULL,
    `application_version` VARCHAR(30) NULL,
    `schema_version` INT UNSIGNED NULL,
    `mariadb_version` VARCHAR(100) NULL,
    `manifest_json` LONGTEXT NULL,
    `error_message` TEXT NULL,
    `created_by_user_id` BIGINT UNSIGNED NULL,
    CONSTRAINT `pk_backup_records` PRIMARY KEY (`record_id`),
    CONSTRAINT `fk_backup_records_created_by_user_id`
        FOREIGN KEY (`created_by_user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE SET NULL,
    CONSTRAINT `chk_backup_records_backup_type_code`
        CHECK (`backup_type_code` IN ('automatic_daily', 'manual', 'pre_migration', 'pre_restore')),
    CONSTRAINT `chk_backup_records_status_code`
        CHECK (`status_code` IN ('in_progress', 'completed', 'failed')),
    CONSTRAINT `chk_backup_records_completed`
        CHECK (`completed_utc` IS NULL OR `completed_utc` >= `started_utc`),
    CONSTRAINT `chk_backup_records_verified`
        CHECK (`verified_utc` IS NULL OR `completed_utc` IS NOT NULL),
    CONSTRAINT `chk_backup_records_restored`
        CHECK (`restored_utc` IS NULL OR `completed_utc` IS NOT NULL),
    CONSTRAINT `chk_backup_records_manifest_json`
        CHECK (`manifest_json` IS NULL OR JSON_VALID(`manifest_json`))
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Backup, verification, retention and restore history for dashboard health reporting.';

CREATE INDEX IF NOT EXISTS `idx_backup_records_status_started`
    ON `backup_records` (`status_code`, `started_utc`);
CREATE INDEX IF NOT EXISTS `idx_backup_records_completed`
    ON `backup_records` (`completed_utc`);

/* ========================================================================== */
/* 2. CUSTOMER TABLES                                                          */
/* ========================================================================== */

CREATE TABLE IF NOT EXISTS `customers` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `company_name` VARCHAR(200) NOT NULL,
    `default_hourly_rate` DECIMAL(18,4) NULL,
    `default_payment_terms_days` SMALLINT UNSIGNED NULL,
    `default_vat_treatment_code` VARCHAR(50) NULL,
    `invoice_delivery_code` VARCHAR(50) NULL,
    `notes` LONGTEXT NULL,
    `is_active` TINYINT(1) NOT NULL DEFAULT 1,
    `date_created_utc` DATETIME(6) NOT NULL,
    `created_by_user_id` BIGINT UNSIGNED NOT NULL,
    `date_updated_utc` DATETIME(6) NOT NULL,
    `updated_by_user_id` BIGINT UNSIGNED NOT NULL,
    `version_no` INT UNSIGNED NOT NULL DEFAULT 1,
    `date_archived_utc` DATETIME(6) NULL,
    CONSTRAINT `pk_customers` PRIMARY KEY (`record_id`),
    CONSTRAINT `fk_customers_created_by_user_id`
        FOREIGN KEY (`created_by_user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_customers_updated_by_user_id`
        FOREIGN KEY (`updated_by_user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `chk_customers_default_hourly_rate`
        CHECK (`default_hourly_rate` IS NULL OR `default_hourly_rate` >= 0),
    CONSTRAINT `chk_customers_default_vat_treatment_code`
        CHECK (`default_vat_treatment_code` IS NULL
            OR `default_vat_treatment_code` IN ('standard', 'zero_rated', 'exempt', 'outside_scope')),
    CONSTRAINT `chk_customers_invoice_delivery_code`
        CHECK (`invoice_delivery_code` IS NULL
            OR `invoice_delivery_code` IN ('email', 'post', 'both', 'manual')),
    CONSTRAINT `chk_customers_is_active` CHECK (`is_active` IN (0, 1)),
    CONSTRAINT `chk_customers_active_archive_state`
        CHECK ((`is_active` = 1 AND `date_archived_utc` IS NULL)
            OR (`is_active` = 0 AND `date_archived_utc` IS NOT NULL)),
    CONSTRAINT `chk_customers_version_no` CHECK (`version_no` >= 1),
    CONSTRAINT `chk_customers_archive_date`
        CHECK (`date_archived_utc` IS NULL OR `date_archived_utc` >= `date_created_utc`)
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Business customers, organisations and individual clients.';

CREATE INDEX IF NOT EXISTS `idx_customers_company_name`
    ON `customers` (`company_name`);
CREATE INDEX IF NOT EXISTS `idx_customers_active_archived`
    ON `customers` (`is_active`, `date_archived_utc`);

CREATE TABLE IF NOT EXISTS `customer_contacts` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `customer_id` BIGINT UNSIGNED NOT NULL,
    `contact_name` VARCHAR(200) NOT NULL,
    `job_title` VARCHAR(150) NULL,
    `email_address` VARCHAR(254) NULL,
    `phone_number` VARCHAR(50) NULL,
    `mobile_number` VARCHAR(50) NULL,
    `is_primary` TINYINT(1) NOT NULL DEFAULT 0,
    `notes` TEXT NULL,
    `date_created_utc` DATETIME(6) NOT NULL,
    `date_updated_utc` DATETIME(6) NOT NULL,
    `version_no` INT UNSIGNED NOT NULL DEFAULT 1,
    `date_archived_utc` DATETIME(6) NULL,
    CONSTRAINT `pk_customer_contacts` PRIMARY KEY (`record_id`),
    CONSTRAINT `fk_customer_contacts_customer_id`
        FOREIGN KEY (`customer_id`) REFERENCES `customers` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `chk_customer_contacts_is_primary` CHECK (`is_primary` IN (0, 1)),
    CONSTRAINT `chk_customer_contacts_version_no` CHECK (`version_no` >= 1),
    CONSTRAINT `chk_customer_contacts_archive_date`
        CHECK (`date_archived_utc` IS NULL OR `date_archived_utc` >= `date_created_utc`)
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Contacts belonging to a customer. The one-primary-contact rule is enforced transactionally in the application service.';

CREATE INDEX IF NOT EXISTS `idx_customer_contacts_customer_id`
    ON `customer_contacts` (`customer_id`);
CREATE INDEX IF NOT EXISTS `idx_customer_contacts_email_address`
    ON `customer_contacts` (`email_address`);
CREATE INDEX IF NOT EXISTS `idx_customer_contacts_name`
    ON `customer_contacts` (`contact_name`);

CREATE TABLE IF NOT EXISTS `customer_addresses` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `customer_id` BIGINT UNSIGNED NOT NULL,
    `address_type_code` VARCHAR(50) NOT NULL,
    `recipient_name` VARCHAR(200) NULL,
    `company_name` VARCHAR(200) NULL,
    `address_line_1` VARCHAR(200) NOT NULL,
    `address_line_2` VARCHAR(200) NULL,
    `town_city` VARCHAR(150) NOT NULL,
    `county` VARCHAR(150) NULL,
    `postcode` VARCHAR(20) NOT NULL,
    `country_code` CHAR(2) NOT NULL DEFAULT 'GB',
    `is_default` TINYINT(1) NOT NULL DEFAULT 0,
    `date_created_utc` DATETIME(6) NOT NULL,
    `date_updated_utc` DATETIME(6) NOT NULL,
    `version_no` INT UNSIGNED NOT NULL DEFAULT 1,
    `date_archived_utc` DATETIME(6) NULL,
    CONSTRAINT `pk_customer_addresses` PRIMARY KEY (`record_id`),
    CONSTRAINT `fk_customer_addresses_customer_id`
        FOREIGN KEY (`customer_id`) REFERENCES `customers` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `chk_customer_addresses_address_type_code`
        CHECK (`address_type_code` IN ('billing', 'service', 'registered', 'other')),
    CONSTRAINT `chk_customer_addresses_is_default` CHECK (`is_default` IN (0, 1)),
    CONSTRAINT `chk_customer_addresses_version_no` CHECK (`version_no` >= 1),
    CONSTRAINT `chk_customer_addresses_archive_date`
        CHECK (`date_archived_utc` IS NULL OR `date_archived_utc` >= `date_created_utc`)
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Customer billing, service/site, registered and other addresses.';

CREATE INDEX IF NOT EXISTS `idx_customer_addresses_customer_type`
    ON `customer_addresses` (`customer_id`, `address_type_code`);
CREATE INDEX IF NOT EXISTS `idx_customer_addresses_postcode`
    ON `customer_addresses` (`postcode`);

/* ========================================================================== */
/* 3. JOB, TIME AND TASK TABLES                                                */
/* ========================================================================== */

CREATE TABLE IF NOT EXISTS `jobs` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `customer_id` BIGINT UNSIGNED NOT NULL,
    `job_number` VARCHAR(60) NOT NULL,
    `job_title` VARCHAR(250) NOT NULL,
    `job_description` LONGTEXT NULL,
    `status_code` VARCHAR(50) NOT NULL DEFAULT 'planned',
    `priority_code` VARCHAR(50) NOT NULL DEFAULT 'normal',
    `charging_type_code` VARCHAR(50) NOT NULL,
    `estimated_hours` DECIMAL(10,2) NULL,
    `agreed_hourly_rate` DECIMAL(18,4) NULL,
    `fixed_price` DECIMAL(18,2) NULL,
    `start_date` DATE NULL,
    `due_date` DATE NULL,
    `completed_utc` DATETIME(6) NULL,
    `notes` LONGTEXT NULL,
    `date_created_utc` DATETIME(6) NOT NULL,
    `created_by_user_id` BIGINT UNSIGNED NOT NULL,
    `date_updated_utc` DATETIME(6) NOT NULL,
    `updated_by_user_id` BIGINT UNSIGNED NOT NULL,
    `version_no` INT UNSIGNED NOT NULL DEFAULT 1,
    `date_archived_utc` DATETIME(6) NULL,
    CONSTRAINT `pk_jobs` PRIMARY KEY (`record_id`),
    CONSTRAINT `uq_jobs_job_number` UNIQUE (`job_number`),
    CONSTRAINT `fk_jobs_customer_id`
        FOREIGN KEY (`customer_id`) REFERENCES `customers` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_jobs_created_by_user_id`
        FOREIGN KEY (`created_by_user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_jobs_updated_by_user_id`
        FOREIGN KEY (`updated_by_user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `chk_jobs_status_code`
        CHECK (`status_code` IN ('planned', 'active', 'on_hold', 'completed', 'cancelled')),
    CONSTRAINT `chk_jobs_priority_code`
        CHECK (`priority_code` IN ('low', 'normal', 'high', 'urgent')),
    CONSTRAINT `chk_jobs_charging_type_code`
        CHECK (`charging_type_code` IN ('hourly', 'fixed_price', 'mixed', 'non_billable')),
    CONSTRAINT `chk_jobs_completion_state`
        CHECK ((`status_code` = 'completed' AND `completed_utc` IS NOT NULL)
            OR (`status_code` <> 'completed' AND `completed_utc` IS NULL)),
    CONSTRAINT `chk_jobs_due_date`
        CHECK (`start_date` IS NULL OR `due_date` IS NULL OR `due_date` >= `start_date`),
    CONSTRAINT `chk_jobs_estimated_hours`
        CHECK (`estimated_hours` IS NULL OR `estimated_hours` >= 0),
    CONSTRAINT `chk_jobs_agreed_hourly_rate`
        CHECK (`agreed_hourly_rate` IS NULL OR `agreed_hourly_rate` >= 0),
    CONSTRAINT `chk_jobs_fixed_price`
        CHECK (`fixed_price` IS NULL OR `fixed_price` >= 0),
    CONSTRAINT `chk_jobs_version_no` CHECK (`version_no` >= 1),
    CONSTRAINT `chk_jobs_archive_date`
        CHECK (`date_archived_utc` IS NULL OR `date_archived_utc` >= `date_created_utc`)
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Customer jobs and their charging, scheduling and workflow state.';

CREATE INDEX IF NOT EXISTS `idx_jobs_customer_status`
    ON `jobs` (`customer_id`, `status_code`);
CREATE INDEX IF NOT EXISTS `idx_jobs_status_due_date`
    ON `jobs` (`status_code`, `due_date`);
CREATE INDEX IF NOT EXISTS `idx_jobs_priority_due_date`
    ON `jobs` (`priority_code`, `due_date`);
CREATE INDEX IF NOT EXISTS `idx_jobs_date_archived_utc`
    ON `jobs` (`date_archived_utc`);
CREATE INDEX IF NOT EXISTS `idx_jobs_title`
    ON `jobs` (`job_title`);

CREATE TABLE IF NOT EXISTS `active_timers` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `user_id` BIGINT UNSIGNED NOT NULL,
    `job_id` BIGINT UNSIGNED NOT NULL,
    `start_utc` DATETIME(6) NOT NULL,
    `work_description` TEXT NULL,
    `is_billable` TINYINT(1) NOT NULL DEFAULT 1,
    `date_created_utc` DATETIME(6) NOT NULL,
    `version_no` INT UNSIGNED NOT NULL DEFAULT 1,
    CONSTRAINT `pk_active_timers` PRIMARY KEY (`record_id`),
    CONSTRAINT `uq_active_timers_user_id` UNIQUE (`user_id`),
    CONSTRAINT `fk_active_timers_user_id`
        FOREIGN KEY (`user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_active_timers_job_id`
        FOREIGN KEY (`job_id`) REFERENCES `jobs` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `chk_active_timers_is_billable` CHECK (`is_billable` IN (0, 1)),
    CONSTRAINT `chk_active_timers_version_no` CHECK (`version_no` >= 1)
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='One persistent running timer per application user.';

CREATE INDEX IF NOT EXISTS `idx_active_timers_job_id`
    ON `active_timers` (`job_id`);

CREATE TABLE IF NOT EXISTS `time_entries` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `user_id` BIGINT UNSIGNED NOT NULL,
    `job_id` BIGINT UNSIGNED NOT NULL,
    `start_utc` DATETIME(6) NOT NULL,
    `end_utc` DATETIME(6) NOT NULL,
    `raw_duration_seconds` BIGINT UNSIGNED NOT NULL,
    `rounded_duration_seconds` BIGINT UNSIGNED NOT NULL,
    `entry_method_code` VARCHAR(50) NOT NULL,
    `is_billable` TINYINT(1) NOT NULL DEFAULT 1,
    `work_description` TEXT NOT NULL,
    `rounding_rule_code` VARCHAR(50) NOT NULL DEFAULT 'none',
    `date_created_utc` DATETIME(6) NOT NULL,
    `created_by_user_id` BIGINT UNSIGNED NOT NULL,
    `date_updated_utc` DATETIME(6) NOT NULL,
    `updated_by_user_id` BIGINT UNSIGNED NOT NULL,
    `version_no` INT UNSIGNED NOT NULL DEFAULT 1,
    CONSTRAINT `pk_time_entries` PRIMARY KEY (`record_id`),
    CONSTRAINT `fk_time_entries_user_id`
        FOREIGN KEY (`user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_time_entries_job_id`
        FOREIGN KEY (`job_id`) REFERENCES `jobs` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_time_entries_created_by_user_id`
        FOREIGN KEY (`created_by_user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_time_entries_updated_by_user_id`
        FOREIGN KEY (`updated_by_user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `chk_time_entries_entry_method_code`
        CHECK (`entry_method_code` IN ('timer', 'manual')),
    CONSTRAINT `chk_time_entries_rounding_rule_code`
        CHECK (`rounding_rule_code` IN (
            'none', 'nearest_5', 'nearest_6', 'nearest_10', 'nearest_15',
            'up_5', 'up_6', 'up_10', 'up_15'
        )),
    CONSTRAINT `chk_time_entries_is_billable` CHECK (`is_billable` IN (0, 1)),
    CONSTRAINT `chk_time_entries_end_after_start` CHECK (`end_utc` > `start_utc`),
    CONSTRAINT `chk_time_entries_positive_duration`
        CHECK (`raw_duration_seconds` > 0 AND `rounded_duration_seconds` > 0),
    CONSTRAINT `chk_time_entries_version_no` CHECK (`version_no` >= 1)
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Completed automatic and manual time records.';

CREATE INDEX IF NOT EXISTS `idx_time_entries_job_start`
    ON `time_entries` (`job_id`, `start_utc`);
CREATE INDEX IF NOT EXISTS `idx_time_entries_user_start`
    ON `time_entries` (`user_id`, `start_utc`);
CREATE INDEX IF NOT EXISTS `idx_time_entries_billable_start`
    ON `time_entries` (`is_billable`, `start_utc`);
CREATE INDEX IF NOT EXISTS `idx_time_entries_start_utc`
    ON `time_entries` (`start_utc`);

CREATE TABLE IF NOT EXISTS `tasks` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `job_id` BIGINT UNSIGNED NULL,
    `task_title` VARCHAR(250) NOT NULL,
    `task_notes` LONGTEXT NULL,
    `status_code` VARCHAR(50) NOT NULL DEFAULT 'not_started',
    `priority_code` VARCHAR(50) NOT NULL DEFAULT 'normal',
    `due_date` DATE NULL,
    `completed_utc` DATETIME(6) NULL,
    `date_created_utc` DATETIME(6) NOT NULL,
    `created_by_user_id` BIGINT UNSIGNED NOT NULL,
    `date_updated_utc` DATETIME(6) NOT NULL,
    `updated_by_user_id` BIGINT UNSIGNED NOT NULL,
    `version_no` INT UNSIGNED NOT NULL DEFAULT 1,
    `date_archived_utc` DATETIME(6) NULL,
    CONSTRAINT `pk_tasks` PRIMARY KEY (`record_id`),
    CONSTRAINT `fk_tasks_job_id`
        FOREIGN KEY (`job_id`) REFERENCES `jobs` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_tasks_created_by_user_id`
        FOREIGN KEY (`created_by_user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_tasks_updated_by_user_id`
        FOREIGN KEY (`updated_by_user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `chk_tasks_status_code`
        CHECK (`status_code` IN ('not_started', 'in_progress', 'blocked', 'completed', 'cancelled')),
    CONSTRAINT `chk_tasks_priority_code`
        CHECK (`priority_code` IN ('low', 'normal', 'high', 'urgent')),
    CONSTRAINT `chk_tasks_completion_state`
        CHECK ((`status_code` = 'completed' AND `completed_utc` IS NOT NULL)
            OR (`status_code` <> 'completed' AND `completed_utc` IS NULL)),
    CONSTRAINT `chk_tasks_version_no` CHECK (`version_no` >= 1),
    CONSTRAINT `chk_tasks_archive_date`
        CHECK (`date_archived_utc` IS NULL OR `date_archived_utc` >= `date_created_utc`)
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='General business tasks and optional job-specific tasks.';

CREATE INDEX IF NOT EXISTS `idx_tasks_job_id`
    ON `tasks` (`job_id`);
CREATE INDEX IF NOT EXISTS `idx_tasks_status_due_date`
    ON `tasks` (`status_code`, `due_date`);
CREATE INDEX IF NOT EXISTS `idx_tasks_priority_due_date`
    ON `tasks` (`priority_code`, `due_date`);
CREATE INDEX IF NOT EXISTS `idx_tasks_title`
    ON `tasks` (`task_title`);

/* ========================================================================== */
/* 4. FINANCIAL ACCOUNT TABLES                                                 */
/* ========================================================================== */

CREATE TABLE IF NOT EXISTS `financial_account_types` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `account_type_code` VARCHAR(80) NOT NULL,
    `display_name` VARCHAR(150) NOT NULL,
    `classification_code` VARCHAR(20) NOT NULL,
    `is_tax_wrapper` TINYINT(1) NOT NULL DEFAULT 0,
    `is_active` TINYINT(1) NOT NULL DEFAULT 1,
    `sort_order` INT UNSIGNED NOT NULL DEFAULT 0,
    CONSTRAINT `pk_financial_account_types` PRIMARY KEY (`record_id`),
    CONSTRAINT `uq_financial_account_types_code` UNIQUE (`account_type_code`),
    CONSTRAINT `chk_financial_account_types_classification`
        CHECK (`classification_code` IN ('asset', 'liability')),
    CONSTRAINT `chk_financial_account_types_is_tax_wrapper`
        CHECK (`is_tax_wrapper` IN (0, 1)),
    CONSTRAINT `chk_financial_account_types_is_active`
        CHECK (`is_active` IN (0, 1))
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Data-driven asset and liability account types.';

CREATE TABLE IF NOT EXISTS `financial_accounts` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `user_id` BIGINT UNSIGNED NOT NULL,
    `account_type_id` BIGINT UNSIGNED NOT NULL,
    `account_scope_code` VARCHAR(20) NOT NULL,
    `provider_name` VARCHAR(200) NOT NULL,
    `account_name` VARCHAR(200) NOT NULL,
    `account_reference_last_four` VARCHAR(4) NULL,
    `currency_code` CHAR(3) NOT NULL DEFAULT 'GBP',
    `account_status_code` VARCHAR(50) NOT NULL DEFAULT 'open',
    `current_balance` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `available_balance` DECIMAL(18,2) NULL,
    `credit_limit` DECIMAL(18,2) NULL,
    `interest_rate` DECIMAL(7,4) NULL,
    `interest_rate_type_code` VARCHAR(50) NULL,
    `introductory_rate_end_date` DATE NULL,
    `fixed_rate_end_date` DATE NULL,
    `maturity_date` DATE NULL,
    `opened_date` DATE NULL,
    `closed_date` DATE NULL,
    `tax_wrapper_code` VARCHAR(50) NULL,
    `provider_reference` VARCHAR(150) NULL,
    `notes` LONGTEXT NULL,
    `last_balance_updated_utc` DATETIME(6) NULL,
    `is_hidden` TINYINT(1) NOT NULL DEFAULT 0,
    `date_created_utc` DATETIME(6) NOT NULL,
    `date_updated_utc` DATETIME(6) NOT NULL,
    `version_no` INT UNSIGNED NOT NULL DEFAULT 1,
    `date_archived_utc` DATETIME(6) NULL,
    CONSTRAINT `pk_financial_accounts` PRIMARY KEY (`record_id`),
    CONSTRAINT `fk_financial_accounts_user_id`
        FOREIGN KEY (`user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_financial_accounts_account_type_id`
        FOREIGN KEY (`account_type_id`) REFERENCES `financial_account_types` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `chk_financial_accounts_scope`
        CHECK (`account_scope_code` IN ('business', 'personal')),
    CONSTRAINT `chk_financial_accounts_status_code`
        CHECK (`account_status_code` IN ('open', 'dormant', 'restricted', 'closed')),
    CONSTRAINT `chk_financial_accounts_interest_rate_type_code`
        CHECK (`interest_rate_type_code` IS NULL
            OR `interest_rate_type_code` IN ('variable', 'fixed', 'tracker', 'promotional')),
    CONSTRAINT `chk_financial_accounts_tax_wrapper_code`
        CHECK (`tax_wrapper_code` IS NULL
            OR `tax_wrapper_code` IN ('cash_isa', 'stocks_shares_isa', 'lifetime_isa', 'pension')),
    CONSTRAINT `chk_financial_accounts_is_hidden` CHECK (`is_hidden` IN (0, 1)),
    CONSTRAINT `chk_financial_accounts_closed_state`
        CHECK ((`account_status_code` = 'closed' AND `closed_date` IS NOT NULL)
            OR (`account_status_code` <> 'closed' AND `closed_date` IS NULL)),
    CONSTRAINT `chk_financial_accounts_credit_limit`
        CHECK (`credit_limit` IS NULL OR `credit_limit` >= 0),
    CONSTRAINT `chk_financial_accounts_dates`
        CHECK (`opened_date` IS NULL OR `closed_date` IS NULL OR `closed_date` >= `opened_date`),
    CONSTRAINT `chk_financial_accounts_version_no` CHECK (`version_no` >= 1),
    CONSTRAINT `chk_financial_accounts_archive_date`
        CHECK (`date_archived_utc` IS NULL OR `date_archived_utc` >= `date_created_utc`)
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Business and personal financial accounts, assets and liabilities.';

CREATE INDEX IF NOT EXISTS `idx_financial_accounts_user_scope`
    ON `financial_accounts` (`user_id`, `account_scope_code`);
CREATE INDEX IF NOT EXISTS `idx_financial_accounts_type_status`
    ON `financial_accounts` (`account_type_id`, `account_status_code`);
CREATE INDEX IF NOT EXISTS `idx_financial_accounts_maturity_date`
    ON `financial_accounts` (`maturity_date`);
CREATE INDEX IF NOT EXISTS `idx_financial_accounts_introductory_end`
    ON `financial_accounts` (`introductory_rate_end_date`);
CREATE INDEX IF NOT EXISTS `idx_financial_accounts_provider_name`
    ON `financial_accounts` (`provider_name`);

CREATE TABLE IF NOT EXISTS `financial_account_balance_snapshots` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `financial_account_id` BIGINT UNSIGNED NOT NULL,
    `balance_at_utc` DATETIME(6) NOT NULL,
    `balance_amount` DECIMAL(18,2) NOT NULL,
    `available_amount` DECIMAL(18,2) NULL,
    `snapshot_source_code` VARCHAR(50) NOT NULL DEFAULT 'manual',
    `notes` TEXT NULL,
    `date_created_utc` DATETIME(6) NOT NULL,
    `created_by_user_id` BIGINT UNSIGNED NOT NULL,
    CONSTRAINT `pk_financial_account_balance_snapshots` PRIMARY KEY (`record_id`),
    CONSTRAINT `fk_financial_account_balance_snapshots_account_id`
        FOREIGN KEY (`financial_account_id`) REFERENCES `financial_accounts` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_financial_account_balance_snapshots_created_by_user_id`
        FOREIGN KEY (`created_by_user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `chk_financial_account_snapshots_source_code`
        CHECK (`snapshot_source_code` IN ('manual', 'statement', 'import', 'system'))
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Historical balance snapshots used for trends and estimated net worth.';

CREATE INDEX IF NOT EXISTS `idx_financial_account_snapshots_account_time`
    ON `financial_account_balance_snapshots` (`financial_account_id`, `balance_at_utc`);
CREATE INDEX IF NOT EXISTS `idx_financial_account_snapshots_balance_time`
    ON `financial_account_balance_snapshots` (`balance_at_utc`);

CREATE TABLE IF NOT EXISTS `financial_account_applications` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `user_id` BIGINT UNSIGNED NOT NULL,
    `account_type_id` BIGINT UNSIGNED NOT NULL,
    `opened_account_id` BIGINT UNSIGNED NULL,
    `provider_name` VARCHAR(200) NOT NULL,
    `product_name` VARCHAR(250) NOT NULL,
    `application_status_code` VARCHAR(50) NOT NULL DEFAULT 'considering',
    `considered_date` DATE NULL,
    `application_date` DATE NULL,
    `decision_date` DATE NULL,
    `expected_open_date` DATE NULL,
    `next_action_date` DATE NULL,
    `application_reference` VARCHAR(150) NULL,
    `advertised_interest_rate` DECIMAL(7,4) NULL,
    `advertised_bonus_amount` DECIMAL(18,2) NULL,
    `introductory_end_date` DATE NULL,
    `application_channel_code` VARCHAR(50) NULL,
    `notes` LONGTEXT NULL,
    `date_created_utc` DATETIME(6) NOT NULL,
    `date_updated_utc` DATETIME(6) NOT NULL,
    `version_no` INT UNSIGNED NOT NULL DEFAULT 1,
    `date_archived_utc` DATETIME(6) NULL,
    CONSTRAINT `pk_financial_account_applications` PRIMARY KEY (`record_id`),
    CONSTRAINT `uq_financial_account_applications_opened_account_id` UNIQUE (`opened_account_id`),
    CONSTRAINT `fk_financial_account_applications_user_id`
        FOREIGN KEY (`user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_financial_account_applications_account_type_id`
        FOREIGN KEY (`account_type_id`) REFERENCES `financial_account_types` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_financial_account_applications_opened_account_id`
        FOREIGN KEY (`opened_account_id`) REFERENCES `financial_accounts` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `chk_financial_account_applications_status_code`
        CHECK (`application_status_code` IN (
            'considering', 'planned', 'applied', 'identity_check', 'awaiting_information',
            'approved', 'declined', 'withdrawn', 'opened', 'completed'
        )),
    CONSTRAINT `chk_financial_account_applications_channel_code`
        CHECK (`application_channel_code` IS NULL
            OR `application_channel_code` IN ('online', 'telephone', 'branch', 'post', 'other')),
    CONSTRAINT `chk_financial_account_applications_opened_account`
        CHECK (`application_status_code` <> 'opened' OR `opened_account_id` IS NOT NULL),
    CONSTRAINT `chk_financial_account_applications_bonus`
        CHECK (`advertised_bonus_amount` IS NULL OR `advertised_bonus_amount` >= 0),
    CONSTRAINT `chk_financial_account_applications_version_no`
        CHECK (`version_no` >= 1),
    CONSTRAINT `chk_financial_account_applications_archive_date`
        CHECK (`date_archived_utc` IS NULL OR `date_archived_utc` >= `date_created_utc`)
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Applications and follow-up dates for prospective financial products.';

CREATE INDEX IF NOT EXISTS `idx_financial_account_applications_status_action`
    ON `financial_account_applications` (`application_status_code`, `next_action_date`);
CREATE INDEX IF NOT EXISTS `idx_financial_account_applications_provider`
    ON `financial_account_applications` (`provider_name`);
CREATE INDEX IF NOT EXISTS `idx_financial_account_applications_application_date`
    ON `financial_account_applications` (`application_date`);

CREATE TABLE IF NOT EXISTS `financial_account_contributions` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `financial_account_id` BIGINT UNSIGNED NOT NULL,
    `contribution_date` DATE NOT NULL,
    `tax_year_start` SMALLINT UNSIGNED NULL,
    `contribution_type_code` VARCHAR(50) NOT NULL,
    `amount` DECIMAL(18,2) NOT NULL,
    `notes` TEXT NULL,
    `date_created_utc` DATETIME(6) NOT NULL,
    `created_by_user_id` BIGINT UNSIGNED NOT NULL,
    CONSTRAINT `pk_financial_account_contributions` PRIMARY KEY (`record_id`),
    CONSTRAINT `fk_financial_account_contributions_account_id`
        FOREIGN KEY (`financial_account_id`) REFERENCES `financial_accounts` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_financial_account_contributions_created_by_user_id`
        FOREIGN KEY (`created_by_user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `chk_financial_account_contributions_type_code`
        CHECK (`contribution_type_code` IN (
            'personal_contribution', 'employer_contribution', 'government_bonus',
            'tax_relief', 'transfer_in', 'other'
        )),
    CONSTRAINT `chk_financial_account_contributions_amount` CHECK (`amount` > 0),
    CONSTRAINT `chk_financial_account_contributions_tax_year`
        CHECK (`tax_year_start` IS NULL OR `tax_year_start` >= 1900)
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Informational contributions to ISAs, savings, investments and other accounts.';

CREATE INDEX IF NOT EXISTS `idx_financial_account_contributions_account_date`
    ON `financial_account_contributions` (`financial_account_id`, `contribution_date`);
CREATE INDEX IF NOT EXISTS `idx_financial_account_contributions_tax_year_date`
    ON `financial_account_contributions` (`tax_year_start`, `contribution_date`);

/* ========================================================================== */
/* 5. INVOICE, CREDIT NOTE AND PAYMENT TABLES                                  */
/* ========================================================================== */

CREATE TABLE IF NOT EXISTS `invoice_number_sequences` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `sequence_code` VARCHAR(50) NOT NULL,
    `number_prefix` VARCHAR(30) NOT NULL,
    `sequence_year` SMALLINT UNSIGNED NOT NULL DEFAULT 0,
    `next_number` BIGINT UNSIGNED NOT NULL DEFAULT 1,
    `version_no` INT UNSIGNED NOT NULL DEFAULT 1,
    CONSTRAINT `pk_invoice_number_sequences` PRIMARY KEY (`record_id`),
    CONSTRAINT `uq_invoice_number_sequences_code_year` UNIQUE (`sequence_code`, `sequence_year`),
    CONSTRAINT `chk_invoice_number_sequences_sequence_code`
        CHECK (`sequence_code` IN ('invoice', 'credit_note')),
    CONSTRAINT `chk_invoice_number_sequences_next_number` CHECK (`next_number` >= 1),
    CONSTRAINT `chk_invoice_number_sequences_version_no` CHECK (`version_no` >= 1)
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Rows locked with SELECT ... FOR UPDATE when allocating invoice and credit-note numbers.';

CREATE TABLE IF NOT EXISTS `invoices` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `invoice_number` VARCHAR(60) NULL,
    `invoice_type_code` VARCHAR(30) NOT NULL DEFAULT 'invoice',
    `credit_for_invoice_id` BIGINT UNSIGNED NULL,
    `customer_id` BIGINT UNSIGNED NOT NULL,
    `status_code` VARCHAR(50) NOT NULL DEFAULT 'draft',
    `invoice_date` DATE NOT NULL,
    `due_date` DATE NOT NULL,
    `finalised_utc` DATETIME(6) NULL,
    `sent_utc` DATETIME(6) NULL,
    `paid_utc` DATETIME(6) NULL,
    `bill_to_name` VARCHAR(200) NULL,
    `bill_to_company` VARCHAR(200) NULL,
    `bill_to_address_line_1` VARCHAR(200) NULL,
    `bill_to_address_line_2` VARCHAR(200) NULL,
    `bill_to_town_city` VARCHAR(150) NULL,
    `bill_to_county` VARCHAR(150) NULL,
    `bill_to_postcode` VARCHAR(20) NULL,
    `bill_to_country_code` CHAR(2) NULL,
    `bill_to_email_address` VARCHAR(254) NULL,
    `currency_code` CHAR(3) NOT NULL DEFAULT 'GBP',
    `prices_include_vat` TINYINT(1) NOT NULL DEFAULT 0,
    `default_vat_rate` DECIMAL(7,4) NULL,
    `net_total` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `vat_total` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `gross_total` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `amount_paid` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `outstanding_amount` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `customer_notes` LONGTEXT NULL,
    `internal_notes` LONGTEXT NULL,
    `payment_instructions` LONGTEXT NULL,
    `pdf_relative_path` VARCHAR(1024) NULL,
    `pdf_sha256_hash` CHAR(64) NULL,
    `pdf_generated_utc` DATETIME(6) NULL,
    `date_created_utc` DATETIME(6) NOT NULL,
    `created_by_user_id` BIGINT UNSIGNED NOT NULL,
    `date_updated_utc` DATETIME(6) NOT NULL,
    `updated_by_user_id` BIGINT UNSIGNED NOT NULL,
    `version_no` INT UNSIGNED NOT NULL DEFAULT 1,
    CONSTRAINT `pk_invoices` PRIMARY KEY (`record_id`),
    CONSTRAINT `uq_invoices_invoice_number` UNIQUE (`invoice_number`),
    CONSTRAINT `fk_invoices_credit_for_invoice_id`
        FOREIGN KEY (`credit_for_invoice_id`) REFERENCES `invoices` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_invoices_customer_id`
        FOREIGN KEY (`customer_id`) REFERENCES `customers` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_invoices_created_by_user_id`
        FOREIGN KEY (`created_by_user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_invoices_updated_by_user_id`
        FOREIGN KEY (`updated_by_user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `chk_invoices_invoice_type_code`
        CHECK (`invoice_type_code` IN ('invoice', 'credit_note')),
    CONSTRAINT `chk_invoices_status_code`
        CHECK (`status_code` IN ('draft', 'finalised', 'sent', 'part_paid', 'paid', 'cancelled', 'credited')),
    CONSTRAINT `chk_invoices_prices_include_vat`
        CHECK (`prices_include_vat` IN (0, 1)),
    CONSTRAINT `chk_invoices_type_credit_reference`
        CHECK ((`invoice_type_code` = 'credit_note' AND `credit_for_invoice_id` IS NOT NULL)
            OR (`invoice_type_code` <> 'credit_note' AND `credit_for_invoice_id` IS NULL)),
    CONSTRAINT `chk_invoices_due_date` CHECK (`due_date` >= `invoice_date`),
    CONSTRAINT `chk_invoices_structural_state`
        CHECK ((`status_code` IN ('draft', 'cancelled')
                AND `invoice_number` IS NULL AND `finalised_utc` IS NULL)
            OR (`status_code` IN ('finalised', 'sent', 'part_paid', 'paid', 'credited')
                AND `invoice_number` IS NOT NULL AND `finalised_utc` IS NOT NULL)),
    CONSTRAINT `chk_invoices_default_vat_rate`
        CHECK (`default_vat_rate` IS NULL OR (`default_vat_rate` >= 0 AND `default_vat_rate` <= 100)),
    CONSTRAINT `chk_invoices_totals_nonnegative`
        CHECK (`net_total` >= 0 AND `vat_total` >= 0 AND `gross_total` >= 0
            AND `amount_paid` >= 0 AND `outstanding_amount` >= 0),
    CONSTRAINT `chk_invoices_gross_total`
        CHECK (`gross_total` = `net_total` + `vat_total`),
    CONSTRAINT `chk_invoices_version_no` CHECK (`version_no` >= 1)
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Draft and immutable finalised invoices and credit notes with billing snapshots.';

CREATE INDEX IF NOT EXISTS `idx_invoices_customer_date`
    ON `invoices` (`customer_id`, `invoice_date`);
CREATE INDEX IF NOT EXISTS `idx_invoices_status_due_date`
    ON `invoices` (`status_code`, `due_date`);
CREATE INDEX IF NOT EXISTS `idx_invoices_invoice_date`
    ON `invoices` (`invoice_date`);
CREATE INDEX IF NOT EXISTS `idx_invoices_credit_for_invoice_id`
    ON `invoices` (`credit_for_invoice_id`);

CREATE TABLE IF NOT EXISTS `invoice_lines` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `invoice_id` BIGINT UNSIGNED NOT NULL,
    `line_number` INT UNSIGNED NOT NULL,
    `line_type_code` VARCHAR(50) NOT NULL,
    `line_description` TEXT NOT NULL,
    `quantity` DECIMAL(18,4) NOT NULL,
    `unit_rate` DECIMAL(18,4) NOT NULL,
    `discount_type_code` VARCHAR(50) NOT NULL DEFAULT 'none',
    `discount_value` DECIMAL(18,4) NOT NULL DEFAULT 0.0000,
    `discount_amount` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `vat_rate` DECIMAL(7,4) NOT NULL DEFAULT 0.0000,
    `net_amount` DECIMAL(18,2) NOT NULL,
    `vat_amount` DECIMAL(18,2) NOT NULL,
    `gross_amount` DECIMAL(18,2) NOT NULL,
    `source_job_id` BIGINT UNSIGNED NULL,
    `credit_for_invoice_line_id` BIGINT UNSIGNED NULL,
    `date_created_utc` DATETIME(6) NOT NULL,
    CONSTRAINT `pk_invoice_lines` PRIMARY KEY (`record_id`),
    CONSTRAINT `uq_invoice_lines_invoice_line_number` UNIQUE (`invoice_id`, `line_number`),
    CONSTRAINT `fk_invoice_lines_invoice_id`
        FOREIGN KEY (`invoice_id`) REFERENCES `invoices` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_invoice_lines_source_job_id`
        FOREIGN KEY (`source_job_id`) REFERENCES `jobs` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_invoice_lines_credit_for_invoice_line_id`
        FOREIGN KEY (`credit_for_invoice_line_id`) REFERENCES `invoice_lines` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `chk_invoice_lines_line_type_code`
        CHECK (`line_type_code` IN ('time', 'fixed_price', 'manual', 'expense_recharge', 'adjustment', 'credit')),
    CONSTRAINT `chk_invoice_lines_discount_type_code`
        CHECK (`discount_type_code` IN ('none', 'percentage', 'fixed_amount')),
    CONSTRAINT `chk_invoice_lines_credit_reference`
        CHECK ((`line_type_code` = 'credit' AND `credit_for_invoice_line_id` IS NOT NULL)
            OR (`line_type_code` <> 'credit' AND `credit_for_invoice_line_id` IS NULL)),
    CONSTRAINT `chk_invoice_lines_line_number` CHECK (`line_number` >= 1),
    CONSTRAINT `chk_invoice_lines_quantity` CHECK (`quantity` <> 0),
    CONSTRAINT `chk_invoice_lines_discount`
        CHECK ((`discount_type_code` = 'none'
                AND `discount_value` = 0 AND `discount_amount` = 0)
            OR (`discount_type_code` = 'percentage'
                AND `discount_value` BETWEEN 0 AND 100 AND `discount_amount` >= 0)
            OR (`discount_type_code` = 'fixed_amount'
                AND `discount_value` >= 0 AND `discount_amount` >= 0)),
    CONSTRAINT `chk_invoice_lines_vat_rate`
        CHECK (`vat_rate` >= 0 AND `vat_rate` <= 100),
    CONSTRAINT `chk_invoice_lines_gross_amount`
        CHECK (`gross_amount` = `net_amount` + `vat_amount`)
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Stored invoice and credit-note lines including exact rounded financial values.';

CREATE INDEX IF NOT EXISTS `idx_invoice_lines_source_job_id`
    ON `invoice_lines` (`source_job_id`);
CREATE INDEX IF NOT EXISTS `idx_invoice_lines_credit_for_line_id`
    ON `invoice_lines` (`credit_for_invoice_line_id`);

CREATE TABLE IF NOT EXISTS `invoice_time_entries` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `invoice_line_id` BIGINT UNSIGNED NOT NULL,
    `time_entry_id` BIGINT UNSIGNED NOT NULL,
    `billed_seconds` BIGINT UNSIGNED NOT NULL,
    `billed_rate` DECIMAL(18,4) NOT NULL,
    `billed_amount` DECIMAL(18,2) NOT NULL,
    `date_created_utc` DATETIME(6) NOT NULL,
    CONSTRAINT `pk_invoice_time_entries` PRIMARY KEY (`record_id`),
    CONSTRAINT `uq_invoice_time_entries_time_entry_id` UNIQUE (`time_entry_id`),
    CONSTRAINT `fk_invoice_time_entries_invoice_line_id`
        FOREIGN KEY (`invoice_line_id`) REFERENCES `invoice_lines` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_invoice_time_entries_time_entry_id`
        FOREIGN KEY (`time_entry_id`) REFERENCES `time_entries` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `chk_invoice_time_entries_duration`
        CHECK (`billed_seconds` > 0),
    CONSTRAINT `chk_invoice_time_entries_values`
        CHECK (`billed_rate` >= 0 AND `billed_amount` >= 0)
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Reservation and immutable billing snapshot for time entries. Unique time_entry_id prevents duplicate invoicing.';

CREATE INDEX IF NOT EXISTS `idx_invoice_time_entries_invoice_line_id`
    ON `invoice_time_entries` (`invoice_line_id`);

CREATE TABLE IF NOT EXISTS `invoice_payments` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `invoice_id` BIGINT UNSIGNED NOT NULL,
    `received_into_account_id` BIGINT UNSIGNED NULL,
    `payment_date` DATE NOT NULL,
    `amount` DECIMAL(18,2) NOT NULL,
    `payment_method_code` VARCHAR(50) NOT NULL,
    `payment_reference` VARCHAR(150) NULL,
    `notes` TEXT NULL,
    `is_reversed` TINYINT(1) NOT NULL DEFAULT 0,
    `reversed_utc` DATETIME(6) NULL,
    `reversal_reason` TEXT NULL,
    `date_created_utc` DATETIME(6) NOT NULL,
    `created_by_user_id` BIGINT UNSIGNED NOT NULL,
    `version_no` INT UNSIGNED NOT NULL DEFAULT 1,
    CONSTRAINT `pk_invoice_payments` PRIMARY KEY (`record_id`),
    CONSTRAINT `fk_invoice_payments_invoice_id`
        FOREIGN KEY (`invoice_id`) REFERENCES `invoices` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_invoice_payments_received_into_account_id`
        FOREIGN KEY (`received_into_account_id`) REFERENCES `financial_accounts` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_invoice_payments_created_by_user_id`
        FOREIGN KEY (`created_by_user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `chk_invoice_payments_method_code`
        CHECK (`payment_method_code` IN (
            'bank_transfer', 'cash', 'debit_card', 'credit_card',
            'direct_debit', 'standing_order', 'cheque', 'other'
        )),
    CONSTRAINT `chk_invoice_payments_is_reversed` CHECK (`is_reversed` IN (0, 1)),
    CONSTRAINT `chk_invoice_payments_amount` CHECK (`amount` > 0),
    CONSTRAINT `chk_invoice_payments_reversal`
        CHECK ((`is_reversed` = 0 AND `reversed_utc` IS NULL)
            OR (`is_reversed` = 1 AND `reversed_utc` IS NOT NULL
                AND NULLIF(TRIM(`reversal_reason`), '') IS NOT NULL)),
    CONSTRAINT `chk_invoice_payments_version_no` CHECK (`version_no` >= 1)
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Invoice payments and append-preserving reversals.';

CREATE INDEX IF NOT EXISTS `idx_invoice_payments_invoice_date`
    ON `invoice_payments` (`invoice_id`, `payment_date`);
CREATE INDEX IF NOT EXISTS `idx_invoice_payments_account_date`
    ON `invoice_payments` (`received_into_account_id`, `payment_date`);

/* ========================================================================== */
/* 6. EXPENSE TABLES                                                           */
/* ========================================================================== */

CREATE TABLE IF NOT EXISTS `expense_categories` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `category_name` VARCHAR(150) NOT NULL,
    `is_active` TINYINT(1) NOT NULL DEFAULT 1,
    `sort_order` INT UNSIGNED NOT NULL DEFAULT 0,
    CONSTRAINT `pk_expense_categories` PRIMARY KEY (`record_id`),
    CONSTRAINT `uq_expense_categories_category_name` UNIQUE (`category_name`),
    CONSTRAINT `chk_expense_categories_is_active` CHECK (`is_active` IN (0, 1))
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='User-configurable business expense categories.';

CREATE TABLE IF NOT EXISTS `expenses` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `expense_date` DATE NOT NULL,
    `supplier_name` VARCHAR(200) NOT NULL,
    `expense_category_id` BIGINT UNSIGNED NOT NULL,
    `paid_from_account_id` BIGINT UNSIGNED NULL,
    `expense_description` TEXT NOT NULL,
    `net_amount` DECIMAL(18,2) NOT NULL,
    `vat_amount` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `gross_amount` DECIMAL(18,2) NOT NULL,
    `payment_method_code` VARCHAR(50) NOT NULL,
    `payment_reference` VARCHAR(150) NULL,
    `is_tax_deductible_estimate` TINYINT(1) NOT NULL DEFAULT 0,
    `notes` LONGTEXT NULL,
    `date_created_utc` DATETIME(6) NOT NULL,
    `created_by_user_id` BIGINT UNSIGNED NOT NULL,
    `date_updated_utc` DATETIME(6) NOT NULL,
    `updated_by_user_id` BIGINT UNSIGNED NOT NULL,
    `version_no` INT UNSIGNED NOT NULL DEFAULT 1,
    `date_archived_utc` DATETIME(6) NULL,
    CONSTRAINT `pk_expenses` PRIMARY KEY (`record_id`),
    CONSTRAINT `fk_expenses_expense_category_id`
        FOREIGN KEY (`expense_category_id`) REFERENCES `expense_categories` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_expenses_paid_from_account_id`
        FOREIGN KEY (`paid_from_account_id`) REFERENCES `financial_accounts` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_expenses_created_by_user_id`
        FOREIGN KEY (`created_by_user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `fk_expenses_updated_by_user_id`
        FOREIGN KEY (`updated_by_user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `chk_expenses_payment_method_code`
        CHECK (`payment_method_code` IN (
            'bank_transfer', 'cash', 'debit_card', 'credit_card',
            'direct_debit', 'standing_order', 'cheque', 'other'
        )),
    CONSTRAINT `chk_expenses_is_tax_deductible`
        CHECK (`is_tax_deductible_estimate` IN (0, 1)),
    CONSTRAINT `chk_expenses_amounts`
        CHECK (`net_amount` >= 0 AND `vat_amount` >= 0 AND `gross_amount` >= 0),
    CONSTRAINT `chk_expenses_gross_amount`
        CHECK (`gross_amount` = `net_amount` + `vat_amount`),
    CONSTRAINT `chk_expenses_version_no` CHECK (`version_no` >= 1),
    CONSTRAINT `chk_expenses_archive_date`
        CHECK (`date_archived_utc` IS NULL OR `date_archived_utc` >= `date_created_utc`)
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Recorded business expenses, VAT estimates and payment-account links.';

CREATE INDEX IF NOT EXISTS `idx_expenses_expense_date`
    ON `expenses` (`expense_date`);
CREATE INDEX IF NOT EXISTS `idx_expenses_category_date`
    ON `expenses` (`expense_category_id`, `expense_date`);
CREATE INDEX IF NOT EXISTS `idx_expenses_account_date`
    ON `expenses` (`paid_from_account_id`, `expense_date`);
CREATE INDEX IF NOT EXISTS `idx_expenses_supplier_name`
    ON `expenses` (`supplier_name`);

/* ========================================================================== */
/* 7. ATTACHMENT TABLES                                                        */
/* ========================================================================== */

CREATE TABLE IF NOT EXISTS `attachments` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `original_file_name` VARCHAR(255) NOT NULL,
    `stored_file_name` VARCHAR(255) NOT NULL,
    `relative_file_path` VARCHAR(1024) NOT NULL,
    `content_type` VARCHAR(150) NULL,
    `file_size_bytes` BIGINT UNSIGNED NOT NULL,
    `sha256_hash` CHAR(64) NOT NULL,
    `attachment_description` TEXT NULL,
    `date_created_utc` DATETIME(6) NOT NULL,
    `created_by_user_id` BIGINT UNSIGNED NOT NULL,
    `date_archived_utc` DATETIME(6) NULL,
    CONSTRAINT `pk_attachments` PRIMARY KEY (`record_id`),
    CONSTRAINT `uq_attachments_stored_file_name` UNIQUE (`stored_file_name`),
    CONSTRAINT `fk_attachments_created_by_user_id`
        FOREIGN KEY (`created_by_user_id`) REFERENCES `users` (`record_id`)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT `chk_attachments_file_size` CHECK (`file_size_bytes` > 0),
    CONSTRAINT `chk_attachments_archive_date`
        CHECK (`date_archived_utc` IS NULL OR `date_archived_utc` >= `date_created_utc`)
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Metadata for files stored outside MariaDB using relative paths and SHA-256 hashes.';

CREATE INDEX IF NOT EXISTS `idx_attachments_sha256_hash`
    ON `attachments` (`sha256_hash`);
CREATE INDEX IF NOT EXISTS `idx_attachments_date_created`
    ON `attachments` (`date_created_utc`);

CREATE TABLE IF NOT EXISTS `customer_attachments` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `customer_id` BIGINT UNSIGNED NOT NULL,
    `attachment_id` BIGINT UNSIGNED NOT NULL,
    `date_created_utc` DATETIME(6) NOT NULL,
    CONSTRAINT `pk_customer_attachments` PRIMARY KEY (`record_id`),
    CONSTRAINT `uq_customer_attachments_link` UNIQUE (`customer_id`, `attachment_id`),
    CONSTRAINT `fk_customer_attachments_customer_id`
        FOREIGN KEY (`customer_id`) REFERENCES `customers` (`record_id`)
        ON UPDATE RESTRICT ON DELETE CASCADE,
    CONSTRAINT `fk_customer_attachments_attachment_id`
        FOREIGN KEY (`attachment_id`) REFERENCES `attachments` (`record_id`)
        ON UPDATE RESTRICT ON DELETE CASCADE
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Links attachments to customers.';

CREATE INDEX IF NOT EXISTS `idx_customer_attachments_attachment_id`
    ON `customer_attachments` (`attachment_id`);

CREATE TABLE IF NOT EXISTS `job_attachments` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `job_id` BIGINT UNSIGNED NOT NULL,
    `attachment_id` BIGINT UNSIGNED NOT NULL,
    `date_created_utc` DATETIME(6) NOT NULL,
    CONSTRAINT `pk_job_attachments` PRIMARY KEY (`record_id`),
    CONSTRAINT `uq_job_attachments_link` UNIQUE (`job_id`, `attachment_id`),
    CONSTRAINT `fk_job_attachments_job_id`
        FOREIGN KEY (`job_id`) REFERENCES `jobs` (`record_id`)
        ON UPDATE RESTRICT ON DELETE CASCADE,
    CONSTRAINT `fk_job_attachments_attachment_id`
        FOREIGN KEY (`attachment_id`) REFERENCES `attachments` (`record_id`)
        ON UPDATE RESTRICT ON DELETE CASCADE
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Links attachments to jobs.';

CREATE INDEX IF NOT EXISTS `idx_job_attachments_attachment_id`
    ON `job_attachments` (`attachment_id`);

CREATE TABLE IF NOT EXISTS `expense_attachments` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `expense_id` BIGINT UNSIGNED NOT NULL,
    `attachment_id` BIGINT UNSIGNED NOT NULL,
    `date_created_utc` DATETIME(6) NOT NULL,
    CONSTRAINT `pk_expense_attachments` PRIMARY KEY (`record_id`),
    CONSTRAINT `uq_expense_attachments_link` UNIQUE (`expense_id`, `attachment_id`),
    CONSTRAINT `fk_expense_attachments_expense_id`
        FOREIGN KEY (`expense_id`) REFERENCES `expenses` (`record_id`)
        ON UPDATE RESTRICT ON DELETE CASCADE,
    CONSTRAINT `fk_expense_attachments_attachment_id`
        FOREIGN KEY (`attachment_id`) REFERENCES `attachments` (`record_id`)
        ON UPDATE RESTRICT ON DELETE CASCADE
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Links receipts and other attachments to expenses.';

CREATE INDEX IF NOT EXISTS `idx_expense_attachments_attachment_id`
    ON `expense_attachments` (`attachment_id`);

CREATE TABLE IF NOT EXISTS `financial_account_attachments` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `financial_account_id` BIGINT UNSIGNED NOT NULL,
    `attachment_id` BIGINT UNSIGNED NOT NULL,
    `date_created_utc` DATETIME(6) NOT NULL,
    CONSTRAINT `pk_financial_account_attachments` PRIMARY KEY (`record_id`),
    CONSTRAINT `uq_financial_account_attachments_link` UNIQUE (`financial_account_id`, `attachment_id`),
    CONSTRAINT `fk_financial_account_attachments_account_id`
        FOREIGN KEY (`financial_account_id`) REFERENCES `financial_accounts` (`record_id`)
        ON UPDATE RESTRICT ON DELETE CASCADE,
    CONSTRAINT `fk_financial_account_attachments_attachment_id`
        FOREIGN KEY (`attachment_id`) REFERENCES `attachments` (`record_id`)
        ON UPDATE RESTRICT ON DELETE CASCADE
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Links attachments to business or personal financial accounts.';

CREATE INDEX IF NOT EXISTS `idx_financial_account_attachments_attachment_id`
    ON `financial_account_attachments` (`attachment_id`);

CREATE TABLE IF NOT EXISTS `financial_account_application_attachments` (
    `record_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `financial_account_application_id` BIGINT UNSIGNED NOT NULL,
    `attachment_id` BIGINT UNSIGNED NOT NULL,
    `date_created_utc` DATETIME(6) NOT NULL,
    CONSTRAINT `pk_financial_account_application_attachments` PRIMARY KEY (`record_id`),
    CONSTRAINT `uq_financial_account_application_attachments_link`
        UNIQUE (`financial_account_application_id`, `attachment_id`),
    CONSTRAINT `fk_financial_account_application_attachments_application_id`
        FOREIGN KEY (`financial_account_application_id`) REFERENCES `financial_account_applications` (`record_id`)
        ON UPDATE RESTRICT ON DELETE CASCADE,
    CONSTRAINT `fk_financial_account_application_attachments_attachment_id`
        FOREIGN KEY (`attachment_id`) REFERENCES `attachments` (`record_id`)
        ON UPDATE RESTRICT ON DELETE CASCADE
) ENGINE=InnoDB
  DEFAULT CHARACTER SET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Links attachments to financial-account applications.';

CREATE INDEX IF NOT EXISTS `idx_financial_account_application_attachments_attachment_id`
    ON `financial_account_application_attachments` (`attachment_id`);

/* ========================================================================== */
/* 8. INITIAL LOOKUP AND CONFIGURATION DATA                                    */
/* ========================================================================== */

START TRANSACTION;

INSERT INTO `schema_information` (
    `record_id`,
    `application_version`,
    `minimum_supported_application_version`,
    `schema_version`,
    `last_verified_utc`,
    `date_updated_utc`
)
VALUES (1, '1.0.0', '1.0.0', 1, NULL, UTC_TIMESTAMP(6))
ON DUPLICATE KEY UPDATE
    `application_version` = VALUES(`application_version`),
    `minimum_supported_application_version` = VALUES(`minimum_supported_application_version`),
    `schema_version` = GREATEST(`schema_version`, VALUES(`schema_version`)),
    `date_updated_utc` = VALUES(`date_updated_utc`);

INSERT INTO `financial_account_types` (
    `account_type_code`, `display_name`, `classification_code`, `is_tax_wrapper`, `is_active`, `sort_order`
)
VALUES
    ('current_account',       'Current Account',             'asset',     0, 1,  10),
    ('savings_account',       'Savings Account',             'asset',     0, 1,  20),
    ('regular_saver',         'Regular Saver',               'asset',     0, 1,  30),
    ('fixed_rate_saver',      'Fixed-Rate Saver',            'asset',     0, 1,  40),
    ('cash_isa',              'Cash ISA',                    'asset',     1, 1,  50),
    ('stocks_shares_isa',     'Stocks and Shares ISA',       'asset',     1, 1,  60),
    ('lifetime_isa',          'Lifetime ISA',                'asset',     1, 1,  70),
    ('investment_account',    'General Investment Account',  'asset',     0, 1,  80),
    ('pension',               'Pension',                     'asset',     1, 1,  90),
    ('cash',                  'Cash',                        'asset',     0, 1, 100),
    ('other_asset',           'Other Asset',                 'asset',     0, 1, 110),
    ('credit_card',           'Credit Card',                 'liability', 0, 1, 200),
    ('overdraft',             'Overdraft',                   'liability', 0, 1, 210),
    ('personal_loan',         'Personal Loan',               'liability', 0, 1, 220),
    ('student_loan',          'Student Loan',                'liability', 0, 1, 230),
    ('mortgage',              'Mortgage',                    'liability', 0, 1, 240),
    ('other_liability',       'Other Liability',             'liability', 0, 1, 250)
ON DUPLICATE KEY UPDATE
    `display_name` = VALUES(`display_name`),
    `classification_code` = VALUES(`classification_code`),
    `is_tax_wrapper` = VALUES(`is_tax_wrapper`),
    `is_active` = VALUES(`is_active`),
    `sort_order` = VALUES(`sort_order`);

INSERT INTO `expense_categories` (`category_name`, `is_active`, `sort_order`)
VALUES ('Uncategorised', 1, 0)
ON DUPLICATE KEY UPDATE
    `is_active` = VALUES(`is_active`),
    `sort_order` = VALUES(`sort_order`);

INSERT INTO `invoice_number_sequences` (
    `sequence_code`, `number_prefix`, `sequence_year`, `next_number`, `version_no`
)
VALUES
    ('invoice', 'INV-', 0, 1, 1),
    ('credit_note', 'CRN-', 0, 1, 1)
ON DUPLICATE KEY UPDATE
    `next_number` = GREATEST(`next_number`, VALUES(`next_number`));

INSERT INTO `application_settings` (
    `setting_key`, `setting_value`, `value_type_code`, `is_sensitive`, `date_updated_utc`, `updated_by_user_id`
)
VALUES
    ('locale',                              'en-GB',  'string',  0, UTC_TIMESTAMP(6), NULL),
    ('default_currency_code',               'GBP',    'string',  0, UTC_TIMESTAMP(6), NULL),
    ('default_country_code',                'GB',     'string',  0, UTC_TIMESTAMP(6), NULL),
    ('theme',                               'dark',   'string',  0, UTC_TIMESTAMP(6), NULL),
    ('default_hourly_rate',                 '0.0000', 'decimal', 0, UTC_TIMESTAMP(6), NULL),
    ('default_payment_terms_days',          '30',     'integer', 0, UTC_TIMESTAMP(6), NULL),
    ('business_vat_registered',             'false',  'boolean', 0, UTC_TIMESTAMP(6), NULL),
    ('vat_registration_number',             '',       'string',  0, UTC_TIMESTAMP(6), NULL),
    ('default_vat_rate',                    '20.0000','decimal', 0, UTC_TIMESTAMP(6), NULL),
    ('prices_include_vat_by_default',       'false',  'boolean', 0, UTC_TIMESTAMP(6), NULL),
    ('default_time_rounding_rule',          'none',   'string',  0, UTC_TIMESTAMP(6), NULL),
    ('forgotten_timer_warning_minutes',     '720',    'integer', 0, UTC_TIMESTAMP(6), NULL),
    ('inactivity_lock_minutes',             '15',     'integer', 0, UTC_TIMESTAMP(6), NULL),
    ('tax_reserve_percentage',              '0.0000', 'decimal', 0, UTC_TIMESTAMP(6), NULL),
    ('automatic_backup_on_first_launch',    'true',   'boolean', 0, UTC_TIMESTAMP(6), NULL),
    ('backup_retention_daily_count',        '7',      'integer', 0, UTC_TIMESTAMP(6), NULL),
    ('backup_retention_weekly_count',       '4',      'integer', 0, UTC_TIMESTAMP(6), NULL),
    ('backup_retention_monthly_count',      '0',      'integer', 0, UTC_TIMESTAMP(6), NULL)
ON DUPLICATE KEY UPDATE
    `value_type_code` = VALUES(`value_type_code`),
    `is_sensitive` = VALUES(`is_sensitive`);

COMMIT;

/* ========================================================================== */
/* 9. OPTIONAL DATABASE ACCOUNTS                                               */
/* ========================================================================== */

/*
    Run the following manually as a MariaDB administrator after replacing the
    placeholder passwords. Keep the runtime account restricted to localhost.

    CREATE USER IF NOT EXISTS 'personal_business_app'@'localhost'
        IDENTIFIED BY 'REPLACE_WITH_A_LONG_RANDOM_PASSWORD';

    GRANT SELECT, INSERT, UPDATE, DELETE
        ON `personal_business_manager`.*
        TO 'personal_business_app'@'localhost';

    CREATE USER IF NOT EXISTS 'personal_business_migrator'@'localhost'
        IDENTIFIED BY 'REPLACE_WITH_A_DIFFERENT_LONG_RANDOM_PASSWORD';

    GRANT SELECT, INSERT, UPDATE, DELETE, CREATE, ALTER, INDEX, DROP,
          REFERENCES, CREATE VIEW, SHOW VIEW, TRIGGER
        ON `personal_business_manager`.*
        TO 'personal_business_migrator'@'localhost';

    FLUSH PRIVILEGES;

    Store application database credentials using Windows Credential Manager or
    Windows DPAPI. Do not commit them to source control or store them in this DB.
*/

/* ========================================================================== */
/* 10. POST-CREATION VERIFICATION QUERIES                                      */
/* ========================================================================== */

/*
    SELECT TABLE_NAME, ENGINE, TABLE_COLLATION
    FROM information_schema.TABLES
    WHERE TABLE_SCHEMA = 'personal_business_manager'
    ORDER BY TABLE_NAME;

    SELECT COUNT(*) AS seeded_financial_account_types
    FROM financial_account_types;

    SELECT setting_key, setting_value, value_type_code
    FROM application_settings
    ORDER BY setting_key;

    The first administrator user must be created by the application setup flow
    using its approved password hasher. Do not insert a plain-text password.
*/

SET SESSION foreign_key_checks = @previous_foreign_key_checks;
SET SESSION time_zone = @previous_time_zone;
SET SESSION sql_mode = @previous_sql_mode;
