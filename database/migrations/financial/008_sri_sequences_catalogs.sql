IF OBJECT_ID('financial.sri_document_sequences', 'U') IS NULL
BEGIN
    CREATE TABLE financial.sri_document_sequences (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        DocumentType nvarchar(32) NOT NULL,
        EstablishmentCode nvarchar(3) NOT NULL,
        EmissionPointCode nvarchar(3) NOT NULL,
        CurrentValue bigint NOT NULL,
        Environment nvarchar(32) NOT NULL,
        IsActive bit NOT NULL,
        CreatedAtUtc datetimeoffset NOT NULL,
        UpdatedAtUtc datetimeoffset NOT NULL
    );
    CREATE UNIQUE INDEX IX_sri_document_sequences_scope ON financial.sri_document_sequences(TenantId, DocumentType, Environment, EstablishmentCode, EmissionPointCode);
END;

IF OBJECT_ID('financial.sri_catalog_items', 'U') IS NULL
BEGIN
    CREATE TABLE financial.sri_catalog_items (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        Catalog nvarchar(64) NOT NULL,
        Code nvarchar(16) NOT NULL,
        Name nvarchar(256) NOT NULL,
        IsActive bit NOT NULL,
        CreatedAtUtc datetimeoffset NOT NULL
    );
    CREATE UNIQUE INDEX IX_sri_catalog_items_Catalog_Code ON financial.sri_catalog_items(Catalog, Code);
END;

IF NOT EXISTS (SELECT 1 FROM financial.sri_catalog_items WHERE Catalog = 'documentType' AND Code = '01')
BEGIN
    INSERT INTO financial.sri_catalog_items (Id, Catalog, Code, Name, IsActive, CreatedAtUtc) VALUES
    (NEWID(), 'documentType', '01', 'Factura', 1, SYSDATETIMEOFFSET()),
    (NEWID(), 'documentType', '04', 'Nota de crédito', 1, SYSDATETIMEOFFSET()),
    (NEWID(), 'documentType', '05', 'Nota de débito', 1, SYSDATETIMEOFFSET()),
    (NEWID(), 'documentType', '06', 'Guía de remisión', 1, SYSDATETIMEOFFSET()),
    (NEWID(), 'documentType', '07', 'Comprobante de retención', 1, SYSDATETIMEOFFSET()),
    (NEWID(), 'identificationType', '04', 'RUC', 1, SYSDATETIMEOFFSET()),
    (NEWID(), 'identificationType', '05', 'Cédula', 1, SYSDATETIMEOFFSET()),
    (NEWID(), 'identificationType', '06', 'Pasaporte', 1, SYSDATETIMEOFFSET()),
    (NEWID(), 'identificationType', '07', 'Consumidor final', 1, SYSDATETIMEOFFSET()),
    (NEWID(), 'identificationType', '08', 'Identificación del exterior', 1, SYSDATETIMEOFFSET()),
    (NEWID(), 'tax', '2', 'IVA', 1, SYSDATETIMEOFFSET()),
    (NEWID(), 'ivaRate', '0', '0%', 1, SYSDATETIMEOFFSET()),
    (NEWID(), 'ivaRate', '2', '12%', 1, SYSDATETIMEOFFSET()),
    (NEWID(), 'ivaRate', '3', '14%', 1, SYSDATETIMEOFFSET()),
    (NEWID(), 'ivaRate', '4', '15%', 1, SYSDATETIMEOFFSET()),
    (NEWID(), 'ivaRate', '6', 'No objeto de impuesto', 1, SYSDATETIMEOFFSET()),
    (NEWID(), 'ivaRate', '7', 'Exento de IVA', 1, SYSDATETIMEOFFSET());
END;
