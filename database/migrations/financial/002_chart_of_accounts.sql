IF OBJECT_ID('financial.accounts', 'U') IS NULL
BEGIN
    CREATE TABLE financial.accounts (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        Code nvarchar(64) NOT NULL,
        Name nvarchar(256) NOT NULL,
        Type nvarchar(32) NOT NULL,
        Level int NOT NULL,
        ParentAccountId uniqueidentifier NULL,
        IsMovementAccount bit NOT NULL,
        Status nvarchar(32) NOT NULL,
        CreatedAtUtc datetimeoffset NOT NULL,
        UpdatedAtUtc datetimeoffset NOT NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_accounts_TenantId_Code' AND object_id = OBJECT_ID('financial.accounts'))
    CREATE UNIQUE INDEX IX_accounts_TenantId_Code ON financial.accounts(TenantId, Code);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_accounts_TenantId_ParentAccountId' AND object_id = OBJECT_ID('financial.accounts'))
    CREATE INDEX IX_accounts_TenantId_ParentAccountId ON financial.accounts(TenantId, ParentAccountId);
