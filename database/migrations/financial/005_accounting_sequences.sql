IF OBJECT_ID('financial.accounting_sequences', 'U') IS NULL
BEGIN
    CREATE TABLE financial.accounting_sequences (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        SequenceKey nvarchar(64) NOT NULL,
        NextValue bigint NOT NULL,
        UpdatedAtUtc datetimeoffset NOT NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_accounting_sequences_TenantId_SequenceKey' AND object_id = OBJECT_ID('financial.accounting_sequences'))
    CREATE UNIQUE INDEX IX_accounting_sequences_TenantId_SequenceKey ON financial.accounting_sequences(TenantId, SequenceKey);
