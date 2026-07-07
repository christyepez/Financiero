IF OBJECT_ID('financial.electronic_documents', 'U') IS NULL
BEGIN
    CREATE TABLE financial.electronic_documents (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        DocumentType nvarchar(32) NOT NULL,
        Environment nvarchar(32) NOT NULL,
        EmissionType nvarchar(32) NOT NULL,
        Status nvarchar(32) NOT NULL,
        EstablishmentCode nvarchar(3) NOT NULL,
        EmissionPointCode nvarchar(3) NOT NULL,
        Sequential nvarchar(9) NULL,
        AccessKey nvarchar(49) NULL,
        IssueDate date NOT NULL,
        CustomerIdentificationType nvarchar(2) NOT NULL,
        CustomerIdentification nvarchar(32) NOT NULL,
        CustomerName nvarchar(256) NOT NULL,
        Currency nvarchar(8) NOT NULL,
        SubtotalWithoutTaxes decimal(19,4) NOT NULL,
        TotalDiscount decimal(19,4) NOT NULL,
        TotalTaxes decimal(19,4) NOT NULL,
        TotalAmount decimal(19,4) NOT NULL,
        XmlContentHash nvarchar(128) NULL,
        SignedXmlContentHash nvarchar(128) NULL,
        SriAuthorizationNumber nvarchar(64) NULL,
        SriAuthorizationDate datetimeoffset NULL,
        SriResponseCode nvarchar(64) NULL,
        SriResponseMessage nvarchar(1024) NULL,
        RelatedJournalEntryId uniqueidentifier NULL,
        CreatedAtUtc datetimeoffset NOT NULL,
        UpdatedAtUtc datetimeoffset NOT NULL,
        GeneratedAtUtc datetimeoffset NULL,
        SignedAtUtc datetimeoffset NULL,
        SentAtUtc datetimeoffset NULL,
        AuthorizedAtUtc datetimeoffset NULL,
        RejectedAtUtc datetimeoffset NULL
    );
    CREATE UNIQUE INDEX IX_electronic_documents_TenantId_AccessKey ON financial.electronic_documents(TenantId, AccessKey) WHERE AccessKey IS NOT NULL;
    CREATE UNIQUE INDEX IX_electronic_documents_TenantId_DocType_Env_Serie_Seq ON financial.electronic_documents(TenantId, DocumentType, Environment, EstablishmentCode, EmissionPointCode, Sequential) WHERE Sequential IS NOT NULL;
    CREATE INDEX IX_electronic_documents_TenantId_Status_IssueDate ON financial.electronic_documents(TenantId, Status, IssueDate);
END;

IF OBJECT_ID('financial.electronic_document_lines', 'U') IS NULL
BEGIN
    CREATE TABLE financial.electronic_document_lines (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        ElectronicDocumentId uniqueidentifier NOT NULL,
        LineNumber int NOT NULL,
        ProductCode nvarchar(64) NOT NULL,
        Description nvarchar(512) NOT NULL,
        Quantity decimal(19,4) NOT NULL,
        UnitPrice decimal(19,4) NOT NULL,
        Discount decimal(19,4) NOT NULL,
        Subtotal decimal(19,4) NOT NULL,
        Total decimal(19,4) NOT NULL,
        CreatedAtUtc datetimeoffset NOT NULL,
        UpdatedAtUtc datetimeoffset NOT NULL,
        CONSTRAINT FK_electronic_document_lines_documents FOREIGN KEY (ElectronicDocumentId) REFERENCES financial.electronic_documents(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_electronic_document_lines_TenantId_Document_Line ON financial.electronic_document_lines(TenantId, ElectronicDocumentId, LineNumber);
END;

IF OBJECT_ID('financial.electronic_document_taxes', 'U') IS NULL
BEGIN
    CREATE TABLE financial.electronic_document_taxes (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        TenantId nvarchar(64) NOT NULL,
        ElectronicDocumentId uniqueidentifier NOT NULL,
        LineId uniqueidentifier NULL,
        TaxCode nvarchar(8) NOT NULL,
        TaxPercentageCode nvarchar(8) NOT NULL,
        TaxRate decimal(19,4) NOT NULL,
        TaxBase decimal(19,4) NOT NULL,
        TaxAmount decimal(19,4) NOT NULL,
        CreatedAtUtc datetimeoffset NOT NULL,
        CONSTRAINT FK_electronic_document_taxes_documents FOREIGN KEY (ElectronicDocumentId) REFERENCES financial.electronic_documents(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_electronic_document_taxes_TenantId_Document ON financial.electronic_document_taxes(TenantId, ElectronicDocumentId);
END;
