IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_journal_entries_TenantId_FiscalPeriodId_Status' AND object_id = OBJECT_ID('financial.journal_entries'))
    CREATE INDEX IX_journal_entries_TenantId_FiscalPeriodId_Status ON financial.journal_entries(TenantId, FiscalPeriodId, Status);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_journal_entries_TenantId_Status_PostingDate' AND object_id = OBJECT_ID('financial.journal_entries'))
    CREATE INDEX IX_journal_entries_TenantId_Status_PostingDate ON financial.journal_entries(TenantId, Status, PostingDate);
