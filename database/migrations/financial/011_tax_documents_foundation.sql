IF OBJECT_ID('financial.electronic_document_references', 'U') IS NULL
BEGIN
    CREATE TABLE financial.electronic_document_references (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId NVARCHAR(64) NOT NULL,
        ElectronicDocumentId UNIQUEIDENTIFIER NOT NULL,
        DocumentTypeCode NVARCHAR(2) NOT NULL,
        Number NVARCHAR(64) NOT NULL,
        IssueDate DATE NOT NULL,
        ReasonOrPeriod NVARCHAR(512) NOT NULL,
        CreatedAtUtc DATETIMEOFFSET NOT NULL,
        CONSTRAINT FK_electronic_document_references_documents FOREIGN KEY (ElectronicDocumentId) REFERENCES financial.electronic_documents(Id) ON DELETE CASCADE
    );
END

IF OBJECT_ID('financial.electronic_document_debit_note_reasons', 'U') IS NULL
BEGIN
    CREATE TABLE financial.electronic_document_debit_note_reasons (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId NVARCHAR(64) NOT NULL,
        ElectronicDocumentId UNIQUEIDENTIFIER NOT NULL,
        Reason NVARCHAR(512) NOT NULL,
        Amount DECIMAL(18, 4) NOT NULL,
        CreatedAtUtc DATETIMEOFFSET NOT NULL,
        CONSTRAINT FK_electronic_document_debit_note_reasons_documents FOREIGN KEY (ElectronicDocumentId) REFERENCES financial.electronic_documents(Id) ON DELETE CASCADE
    );
END

IF OBJECT_ID('financial.electronic_document_withholding_taxes', 'U') IS NULL
BEGIN
    CREATE TABLE financial.electronic_document_withholding_taxes (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId NVARCHAR(64) NOT NULL,
        ElectronicDocumentId UNIQUEIDENTIFIER NOT NULL,
        TaxCode NVARCHAR(16) NOT NULL,
        WithholdingCode NVARCHAR(16) NOT NULL,
        TaxBase DECIMAL(18, 4) NOT NULL,
        WithholdingPercentage DECIMAL(18, 4) NOT NULL,
        WithheldAmount DECIMAL(18, 4) NOT NULL,
        SupportDocumentNumber NVARCHAR(64) NOT NULL,
        SupportDocumentIssueDate DATE NOT NULL,
        FiscalPeriod NVARCHAR(16) NOT NULL,
        CreatedAtUtc DATETIMEOFFSET NOT NULL,
        CONSTRAINT FK_electronic_document_withholding_taxes_documents FOREIGN KEY (ElectronicDocumentId) REFERENCES financial.electronic_documents(Id) ON DELETE CASCADE
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_electronic_document_references_tenant_document' AND object_id = OBJECT_ID('financial.electronic_document_references'))
    CREATE INDEX IX_electronic_document_references_tenant_document ON financial.electronic_document_references (TenantId, ElectronicDocumentId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_electronic_document_references_tenant_number' AND object_id = OBJECT_ID('financial.electronic_document_references'))
    CREATE INDEX IX_electronic_document_references_tenant_number ON financial.electronic_document_references (TenantId, Number);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_electronic_document_debit_note_reasons_tenant_document' AND object_id = OBJECT_ID('financial.electronic_document_debit_note_reasons'))
    CREATE INDEX IX_electronic_document_debit_note_reasons_tenant_document ON financial.electronic_document_debit_note_reasons (TenantId, ElectronicDocumentId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_electronic_document_withholding_taxes_tenant_document' AND object_id = OBJECT_ID('financial.electronic_document_withholding_taxes'))
    CREATE INDEX IX_electronic_document_withholding_taxes_tenant_document ON financial.electronic_document_withholding_taxes (TenantId, ElectronicDocumentId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_electronic_document_withholding_taxes_tenant_period' AND object_id = OBJECT_ID('financial.electronic_document_withholding_taxes'))
    CREATE INDEX IX_electronic_document_withholding_taxes_tenant_period ON financial.electronic_document_withholding_taxes (TenantId, FiscalPeriod);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_electronic_documents_tenant_type_issue_date' AND object_id = OBJECT_ID('financial.electronic_documents'))
    CREATE INDEX IX_electronic_documents_tenant_type_issue_date ON financial.electronic_documents (TenantId, DocumentType, IssueDate);
