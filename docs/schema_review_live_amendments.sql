/*
    One-time development-database amendment script for schema review SR-001..SR-011.

    Preconditions:
      - Take and verify a backup.
      - Confirm tasks.recurrence_definition_id and expenses.payment_method_code nulls.
      - Validate all existing closed-code values.
      - Run against the personal_business_manager development database only.

    The empty-database source of truth remains
    personal_business_management_application_schema.sql.
*/

USE `personal_business_manager`;

SET @previous_sql_mode := @@SESSION.sql_mode;
SET @previous_time_zone := @@SESSION.time_zone;
SET SESSION sql_mode = 'STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';
SET SESSION time_zone = '+00:00';

ALTER TABLE `time_entries`
    ADD COLUMN `rounded_duration_seconds` BIGINT UNSIGNED NULL AFTER `raw_duration_seconds`;

UPDATE `time_entries`
SET `rounded_duration_seconds` =
    CASE
        WHEN `rounding_rule_code` = 'none' THEN `raw_duration_seconds`
        ELSE `rounded_duration_minutes` * 60
    END;

ALTER TABLE `time_entries`
    MODIFY COLUMN `rounded_duration_seconds` BIGINT UNSIGNED NOT NULL,
    DROP COLUMN `rounded_duration_minutes`,
    DROP CONSTRAINT `chk_time_entries_positive_duration`,
    ADD CONSTRAINT `chk_time_entries_positive_duration`
        CHECK (`raw_duration_seconds` > 0 AND `rounded_duration_seconds` > 0),
    ADD CONSTRAINT `chk_time_entries_entry_method_code`
        CHECK (`entry_method_code` IN ('timer', 'manual')),
    ADD CONSTRAINT `chk_time_entries_rounding_rule_code`
        CHECK (`rounding_rule_code` IN (
            'none', 'nearest_5', 'nearest_6', 'nearest_10', 'nearest_15',
            'up_5', 'up_6', 'up_10', 'up_15'
        )),
    ADD CONSTRAINT `chk_time_entries_is_billable` CHECK (`is_billable` IN (0, 1));

ALTER TABLE `invoice_time_entries`
    DROP COLUMN `billed_minutes`,
    DROP CONSTRAINT `chk_invoice_time_entries_duration`,
    ADD CONSTRAINT `chk_invoice_time_entries_duration`
        CHECK (`billed_seconds` > 0);

ALTER TABLE `tasks`
    DROP COLUMN `recurrence_definition_id`,
    ADD CONSTRAINT `chk_tasks_status_code`
        CHECK (`status_code` IN ('not_started', 'in_progress', 'blocked', 'completed', 'cancelled')),
    ADD CONSTRAINT `chk_tasks_priority_code`
        CHECK (`priority_code` IN ('low', 'normal', 'high', 'urgent')),
    ADD CONSTRAINT `chk_tasks_completion_state`
        CHECK ((`status_code` = 'completed' AND `completed_utc` IS NOT NULL)
            OR (`status_code` <> 'completed' AND `completed_utc` IS NULL));

ALTER TABLE `audit_records`
    MODIFY COLUMN `user_id` BIGINT UNSIGNED NULL;

ALTER TABLE `expenses`
    MODIFY COLUMN `payment_method_code` VARCHAR(50) NOT NULL,
    ADD CONSTRAINT `chk_expenses_payment_method_code`
        CHECK (`payment_method_code` IN (
            'bank_transfer', 'cash', 'debit_card', 'credit_card',
            'direct_debit', 'standing_order', 'cheque', 'other'
        )),
    ADD CONSTRAINT `chk_expenses_is_tax_deductible`
        CHECK (`is_tax_deductible_estimate` IN (0, 1));

ALTER TABLE `users`
    ADD CONSTRAINT `chk_users_role_code` CHECK (`role_code` IN ('administrator')),
    ADD CONSTRAINT `chk_users_is_active` CHECK (`is_active` IN (0, 1)),
    ADD CONSTRAINT `chk_users_password_changed`
        CHECK (`password_changed_utc` >= `date_created_utc`);

ALTER TABLE `application_settings`
    ADD CONSTRAINT `chk_application_settings_value_type_code`
        CHECK (`value_type_code` IN ('string', 'integer', 'decimal', 'boolean', 'date', 'datetime', 'json')),
    ADD CONSTRAINT `chk_application_settings_is_sensitive`
        CHECK (`is_sensitive` IN (0, 1));

ALTER TABLE `backup_records`
    ADD CONSTRAINT `chk_backup_records_backup_type_code`
        CHECK (`backup_type_code` IN ('automatic_daily', 'manual', 'pre_migration', 'pre_restore')),
    ADD CONSTRAINT `chk_backup_records_status_code`
        CHECK (`status_code` IN ('in_progress', 'completed', 'failed'));

ALTER TABLE `customers`
    ADD CONSTRAINT `chk_customers_default_vat_treatment_code`
        CHECK (`default_vat_treatment_code` IS NULL
            OR `default_vat_treatment_code` IN ('standard', 'zero_rated', 'exempt', 'outside_scope')),
    ADD CONSTRAINT `chk_customers_invoice_delivery_code`
        CHECK (`invoice_delivery_code` IS NULL
            OR `invoice_delivery_code` IN ('email', 'post', 'both', 'manual')),
    ADD CONSTRAINT `chk_customers_is_active` CHECK (`is_active` IN (0, 1)),
    ADD CONSTRAINT `chk_customers_active_archive_state`
        CHECK ((`is_active` = 1 AND `date_archived_utc` IS NULL)
            OR (`is_active` = 0 AND `date_archived_utc` IS NOT NULL));

ALTER TABLE `customer_contacts`
    ADD CONSTRAINT `chk_customer_contacts_is_primary` CHECK (`is_primary` IN (0, 1));

ALTER TABLE `customer_addresses`
    ADD CONSTRAINT `chk_customer_addresses_address_type_code`
        CHECK (`address_type_code` IN ('billing', 'service', 'registered', 'other')),
    ADD CONSTRAINT `chk_customer_addresses_is_default` CHECK (`is_default` IN (0, 1));

ALTER TABLE `jobs`
    ADD CONSTRAINT `chk_jobs_status_code`
        CHECK (`status_code` IN ('planned', 'active', 'on_hold', 'completed', 'cancelled')),
    ADD CONSTRAINT `chk_jobs_priority_code`
        CHECK (`priority_code` IN ('low', 'normal', 'high', 'urgent')),
    ADD CONSTRAINT `chk_jobs_charging_type_code`
        CHECK (`charging_type_code` IN ('hourly', 'fixed_price', 'mixed', 'non_billable')),
    ADD CONSTRAINT `chk_jobs_completion_state`
        CHECK ((`status_code` = 'completed' AND `completed_utc` IS NOT NULL)
            OR (`status_code` <> 'completed' AND `completed_utc` IS NULL));

ALTER TABLE `active_timers`
    ADD CONSTRAINT `chk_active_timers_is_billable` CHECK (`is_billable` IN (0, 1));

ALTER TABLE `financial_account_types`
    ADD CONSTRAINT `chk_financial_account_types_is_tax_wrapper`
        CHECK (`is_tax_wrapper` IN (0, 1)),
    ADD CONSTRAINT `chk_financial_account_types_is_active`
        CHECK (`is_active` IN (0, 1));

ALTER TABLE `financial_accounts`
    ADD CONSTRAINT `chk_financial_accounts_status_code`
        CHECK (`account_status_code` IN ('open', 'dormant', 'restricted', 'closed')),
    ADD CONSTRAINT `chk_financial_accounts_interest_rate_type_code`
        CHECK (`interest_rate_type_code` IS NULL
            OR `interest_rate_type_code` IN ('variable', 'fixed', 'tracker', 'promotional')),
    ADD CONSTRAINT `chk_financial_accounts_tax_wrapper_code`
        CHECK (`tax_wrapper_code` IS NULL
            OR `tax_wrapper_code` IN ('cash_isa', 'stocks_shares_isa', 'lifetime_isa', 'pension')),
    ADD CONSTRAINT `chk_financial_accounts_is_hidden` CHECK (`is_hidden` IN (0, 1)),
    ADD CONSTRAINT `chk_financial_accounts_closed_state`
        CHECK ((`account_status_code` = 'closed' AND `closed_date` IS NOT NULL)
            OR (`account_status_code` <> 'closed' AND `closed_date` IS NULL));

ALTER TABLE `financial_account_balance_snapshots`
    ADD CONSTRAINT `chk_financial_account_snapshots_source_code`
        CHECK (`snapshot_source_code` IN ('manual', 'statement', 'import', 'system'));

ALTER TABLE `financial_account_applications`
    ADD CONSTRAINT `chk_financial_account_applications_status_code`
        CHECK (`application_status_code` IN (
            'considering', 'planned', 'applied', 'identity_check', 'awaiting_information',
            'approved', 'declined', 'withdrawn', 'opened', 'completed'
        )),
    ADD CONSTRAINT `chk_financial_account_applications_channel_code`
        CHECK (`application_channel_code` IS NULL
            OR `application_channel_code` IN ('online', 'telephone', 'branch', 'post', 'other')),
    ADD CONSTRAINT `chk_financial_account_applications_opened_account`
        CHECK (`application_status_code` <> 'opened' OR `opened_account_id` IS NOT NULL);

ALTER TABLE `financial_account_contributions`
    ADD CONSTRAINT `chk_financial_account_contributions_type_code`
        CHECK (`contribution_type_code` IN (
            'personal_contribution', 'employer_contribution', 'government_bonus',
            'tax_relief', 'transfer_in', 'other'
        ));

ALTER TABLE `invoice_number_sequences`
    ADD CONSTRAINT `chk_invoice_number_sequences_sequence_code`
        CHECK (`sequence_code` IN ('invoice', 'credit_note'));

ALTER TABLE `invoices`
    DROP CONSTRAINT `chk_invoices_finalised_number`,
    ADD CONSTRAINT `chk_invoices_invoice_type_code`
        CHECK (`invoice_type_code` IN ('invoice', 'credit_note')),
    ADD CONSTRAINT `chk_invoices_status_code`
        CHECK (`status_code` IN ('draft', 'finalised', 'sent', 'part_paid', 'paid', 'cancelled', 'credited')),
    ADD CONSTRAINT `chk_invoices_prices_include_vat`
        CHECK (`prices_include_vat` IN (0, 1)),
    ADD CONSTRAINT `chk_invoices_structural_state`
        CHECK ((`status_code` IN ('draft', 'cancelled')
                AND `invoice_number` IS NULL AND `finalised_utc` IS NULL)
            OR (`status_code` IN ('finalised', 'sent', 'part_paid', 'paid', 'credited')
                AND `invoice_number` IS NOT NULL AND `finalised_utc` IS NOT NULL));

ALTER TABLE `invoice_lines`
    DROP CONSTRAINT `chk_invoice_lines_discount`,
    ADD CONSTRAINT `chk_invoice_lines_line_type_code`
        CHECK (`line_type_code` IN ('time', 'fixed_price', 'manual', 'expense_recharge', 'adjustment', 'credit')),
    ADD CONSTRAINT `chk_invoice_lines_discount_type_code`
        CHECK (`discount_type_code` IN ('none', 'percentage', 'fixed_amount')),
    ADD CONSTRAINT `chk_invoice_lines_credit_reference`
        CHECK ((`line_type_code` = 'credit' AND `credit_for_invoice_line_id` IS NOT NULL)
            OR (`line_type_code` <> 'credit' AND `credit_for_invoice_line_id` IS NULL)),
    ADD CONSTRAINT `chk_invoice_lines_discount`
        CHECK ((`discount_type_code` = 'none'
                AND `discount_value` = 0 AND `discount_amount` = 0)
            OR (`discount_type_code` = 'percentage'
                AND `discount_value` BETWEEN 0 AND 100 AND `discount_amount` >= 0)
            OR (`discount_type_code` = 'fixed_amount'
                AND `discount_value` >= 0 AND `discount_amount` >= 0));

ALTER TABLE `invoice_payments`
    DROP CONSTRAINT `chk_invoice_payments_reversal`,
    ADD CONSTRAINT `chk_invoice_payments_method_code`
        CHECK (`payment_method_code` IN (
            'bank_transfer', 'cash', 'debit_card', 'credit_card',
            'direct_debit', 'standing_order', 'cheque', 'other'
        )),
    ADD CONSTRAINT `chk_invoice_payments_is_reversed` CHECK (`is_reversed` IN (0, 1)),
    ADD CONSTRAINT `chk_invoice_payments_reversal`
        CHECK ((`is_reversed` = 0 AND `reversed_utc` IS NULL)
            OR (`is_reversed` = 1 AND `reversed_utc` IS NOT NULL
                AND NULLIF(TRIM(`reversal_reason`), '') IS NOT NULL));

ALTER TABLE `expense_categories`
    ADD CONSTRAINT `chk_expense_categories_is_active` CHECK (`is_active` IN (0, 1));

SET SESSION sql_mode = @previous_sql_mode;
SET SESSION time_zone = @previous_time_zone;
