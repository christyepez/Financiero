IF COL_LENGTH('financial.electronic_documents', 'RideGeneratedAtUtc') IS NULL
BEGIN
    ALTER TABLE financial.electronic_documents ADD
        RideGeneratedAtUtc datetimeoffset NULL,
        RidePdfHash nvarchar(128) NULL,
        StorageProvider nvarchar(64) NULL,
        SriReceptionAttempts int NOT NULL CONSTRAINT DF_electronic_documents_SriReceptionAttempts DEFAULT 0,
        SriAuthorizationAttempts int NOT NULL CONSTRAINT DF_electronic_documents_SriAuthorizationAttempts DEFAULT 0,
        LastIntegrationCorrelationId nvarchar(128) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_electronic_documents_TenantId_RideGeneratedAtUtc' AND object_id = OBJECT_ID('financial.electronic_documents'))
BEGIN
    CREATE INDEX IX_electronic_documents_TenantId_RideGeneratedAtUtc ON financial.electronic_documents(TenantId, RideGeneratedAtUtc);
END;
