IF OBJECT_ID('financial.fiscal_years', 'U') IS NULL
BEGIN
    CREATE TABLE financial.fiscal_years (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        Year int NOT NULL,
        StartDate date NOT NULL,
        EndDate date NOT NULL,
        Status nvarchar(32) NOT NULL,
        CreatedAtUtc datetimeoffset NOT NULL,
        UpdatedAtUtc datetimeoffset NOT NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_fiscal_years_TenantId_Year' AND object_id = OBJECT_ID('financial.fiscal_years'))
    CREATE UNIQUE INDEX IX_fiscal_years_TenantId_Year ON financial.fiscal_years(TenantId, Year);

IF OBJECT_ID('financial.fiscal_periods', 'U') IS NULL
BEGIN
    CREATE TABLE financial.fiscal_periods (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        FiscalYearId uniqueidentifier NOT NULL,
        PeriodNumber int NOT NULL,
        StartDate date NOT NULL,
        EndDate date NOT NULL,
        Status nvarchar(32) NOT NULL,
        CreatedAtUtc datetimeoffset NOT NULL,
        UpdatedAtUtc datetimeoffset NOT NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_fiscal_periods_TenantId_FiscalYearId_PeriodNumber' AND object_id = OBJECT_ID('financial.fiscal_periods'))
    CREATE UNIQUE INDEX IX_fiscal_periods_TenantId_FiscalYearId_PeriodNumber ON financial.fiscal_periods(TenantId, FiscalYearId, PeriodNumber);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_fiscal_periods_TenantId_FiscalYearId_StartDate_EndDate' AND object_id = OBJECT_ID('financial.fiscal_periods'))
    CREATE INDEX IX_fiscal_periods_TenantId_FiscalYearId_StartDate_EndDate ON financial.fiscal_periods(TenantId, FiscalYearId, StartDate, EndDate);
