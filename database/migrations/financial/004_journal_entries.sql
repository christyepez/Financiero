IF OBJECT_ID('financial.journal_entries', 'U') IS NULL
BEGIN
    CREATE TABLE financial.journal_entries (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        FiscalPeriodId uniqueidentifier NULL,
        EntryNumber nvarchar(64) NULL,
        PostingDate date NOT NULL,
        Reference nvarchar(128) NULL,
        Description nvarchar(512) NOT NULL,
        Status nvarchar(32) NOT NULL,
        Source nvarchar(32) NOT NULL,
        ReversalOfJournalEntryId uniqueidentifier NULL,
        ReversedByJournalEntryId uniqueidentifier NULL,
        PostedAtUtc datetimeoffset NULL,
        ReversedAtUtc datetimeoffset NULL,
        VoidedAtUtc datetimeoffset NULL,
        Reason nvarchar(512) NULL,
        CreatedAtUtc datetimeoffset NOT NULL,
        UpdatedAtUtc datetimeoffset NOT NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_journal_entries_TenantId_EntryNumber' AND object_id = OBJECT_ID('financial.journal_entries'))
    CREATE UNIQUE INDEX IX_journal_entries_TenantId_EntryNumber ON financial.journal_entries(TenantId, EntryNumber) WHERE EntryNumber IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_journal_entries_TenantId_PostingDate_Status' AND object_id = OBJECT_ID('financial.journal_entries'))
    CREATE INDEX IX_journal_entries_TenantId_PostingDate_Status ON financial.journal_entries(TenantId, PostingDate, Status);

IF OBJECT_ID('financial.journal_entry_lines', 'U') IS NULL
BEGIN
    CREATE TABLE financial.journal_entry_lines (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        JournalEntryId uniqueidentifier NOT NULL,
        TenantId nvarchar(64) NOT NULL,
        AccountId uniqueidentifier NOT NULL,
        LineNumber int NOT NULL,
        Description nvarchar(512) NULL,
        Debit decimal(19,4) NOT NULL,
        Credit decimal(19,4) NOT NULL,
        CreatedAtUtc datetimeoffset NOT NULL,
        UpdatedAtUtc datetimeoffset NOT NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_journal_entry_lines_journal_entries')
    ALTER TABLE financial.journal_entry_lines ADD CONSTRAINT FK_journal_entry_lines_journal_entries FOREIGN KEY (JournalEntryId) REFERENCES financial.journal_entries(Id) ON DELETE CASCADE;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_journal_entry_lines_TenantId_JournalEntryId_LineNumber' AND object_id = OBJECT_ID('financial.journal_entry_lines'))
    CREATE UNIQUE INDEX IX_journal_entry_lines_TenantId_JournalEntryId_LineNumber ON financial.journal_entry_lines(TenantId, JournalEntryId, LineNumber);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_journal_entry_lines_TenantId_AccountId' AND object_id = OBJECT_ID('financial.journal_entry_lines'))
    CREATE INDEX IX_journal_entry_lines_TenantId_AccountId ON financial.journal_entry_lines(TenantId, AccountId);
