IF COL_LENGTH('financial.electronic_documents', 'UnsignedXmlStorageId') IS NULL
BEGIN
    ALTER TABLE financial.electronic_documents ADD
        UnsignedXmlStorageId nvarchar(256) NULL,
        SignedXmlStorageId nvarchar(256) NULL,
        AuthorizationXmlStorageId nvarchar(256) NULL,
        RidePdfStorageId nvarchar(256) NULL,
        SignatureProvider nvarchar(64) NULL,
        SignatureDigest nvarchar(128) NULL,
        LastSriStatus nvarchar(64) NULL,
        LastSriMessage nvarchar(1024) NULL,
        LastErrorCode nvarchar(128) NULL,
        LastErrorMessage nvarchar(1024) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_electronic_documents_TenantId_LastSriStatus' AND object_id = OBJECT_ID('financial.electronic_documents'))
BEGIN
    CREATE INDEX IX_electronic_documents_TenantId_LastSriStatus ON financial.electronic_documents(TenantId, LastSriStatus);
END;
