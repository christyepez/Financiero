IF OBJECT_ID('financial.purchase_tax_documents', 'U') IS NULL
BEGIN
    CREATE TABLE financial.purchase_tax_documents (
        id uniqueidentifier NOT NULL CONSTRAINT pk_purchase_tax_documents PRIMARY KEY,
        tenant_id nvarchar(64) NOT NULL,
        supplier_identification_type nvarchar(2) NOT NULL,
        supplier_identification nvarchar(32) NOT NULL,
        supplier_name nvarchar(256) NOT NULL,
        establishment nvarchar(3) NOT NULL,
        emission_point nvarchar(3) NOT NULL,
        sequential nvarchar(9) NOT NULL,
        document_type nvarchar(2) NOT NULL,
        authorization_number nvarchar(64) NULL,
        access_key nvarchar(49) NULL,
        issue_date date NOT NULL,
        registration_date date NOT NULL,
        fiscal_period nvarchar(7) NOT NULL,
        support_document_type nvarchar(2) NOT NULL,
        subtotal decimal(19,4) NOT NULL,
        tax_total decimal(19,4) NOT NULL,
        total decimal(19,4) NOT NULL,
        currency nvarchar(8) NOT NULL,
        status nvarchar(32) NOT NULL,
        created_at_utc datetimeoffset NOT NULL,
        updated_at_utc datetimeoffset NOT NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ux_purchase_tax_documents_number' AND object_id = OBJECT_ID('financial.purchase_tax_documents'))
    CREATE UNIQUE INDEX ux_purchase_tax_documents_number ON financial.purchase_tax_documents(tenant_id, document_type, establishment, emission_point, sequential);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ix_purchase_tax_documents_period' AND object_id = OBJECT_ID('financial.purchase_tax_documents'))
    CREATE INDEX ix_purchase_tax_documents_period ON financial.purchase_tax_documents(tenant_id, fiscal_period);

IF OBJECT_ID('financial.purchase_tax_document_lines', 'U') IS NULL
BEGIN
    CREATE TABLE financial.purchase_tax_document_lines (
        id uniqueidentifier NOT NULL CONSTRAINT pk_purchase_tax_document_lines PRIMARY KEY,
        purchase_tax_document_id uniqueidentifier NOT NULL,
        tenant_id nvarchar(64) NOT NULL,
        line_number int NOT NULL,
        product_code nvarchar(64) NOT NULL,
        description nvarchar(512) NOT NULL,
        quantity decimal(19,4) NOT NULL,
        unit_price decimal(19,4) NOT NULL,
        discount decimal(19,4) NOT NULL,
        subtotal decimal(19,4) NOT NULL,
        CONSTRAINT fk_purchase_lines_document FOREIGN KEY (purchase_tax_document_id) REFERENCES financial.purchase_tax_documents(id) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ux_purchase_tax_document_lines_number' AND object_id = OBJECT_ID('financial.purchase_tax_document_lines'))
    CREATE UNIQUE INDEX ux_purchase_tax_document_lines_number ON financial.purchase_tax_document_lines(tenant_id, purchase_tax_document_id, line_number);

IF OBJECT_ID('financial.purchase_taxes', 'U') IS NULL
BEGIN
    CREATE TABLE financial.purchase_taxes (
        id uniqueidentifier NOT NULL CONSTRAINT pk_purchase_taxes PRIMARY KEY,
        purchase_tax_document_id uniqueidentifier NOT NULL,
        tenant_id nvarchar(64) NOT NULL,
        tax_code nvarchar(8) NOT NULL,
        tax_percentage_code nvarchar(8) NOT NULL,
        tax_base decimal(19,4) NOT NULL,
        tax_rate decimal(19,4) NOT NULL,
        tax_amount decimal(19,4) NOT NULL,
        CONSTRAINT fk_purchase_taxes_document FOREIGN KEY (purchase_tax_document_id) REFERENCES financial.purchase_tax_documents(id) ON DELETE CASCADE
    );
END;

IF OBJECT_ID('financial.purchase_support_document_references', 'U') IS NULL
BEGIN
    CREATE TABLE financial.purchase_support_document_references (
        id uniqueidentifier NOT NULL CONSTRAINT pk_purchase_support_document_references PRIMARY KEY,
        purchase_tax_document_id uniqueidentifier NOT NULL,
        tenant_id nvarchar(64) NOT NULL,
        document_type_code nvarchar(2) NOT NULL,
        number nvarchar(64) NOT NULL,
        issue_date date NOT NULL,
        reason nvarchar(512) NOT NULL,
        CONSTRAINT fk_purchase_references_document FOREIGN KEY (purchase_tax_document_id) REFERENCES financial.purchase_tax_documents(id) ON DELETE CASCADE
    );
END;

IF OBJECT_ID('financial.voided_tax_documents', 'U') IS NULL
BEGIN
    CREATE TABLE financial.voided_tax_documents (
        id uniqueidentifier NOT NULL CONSTRAINT pk_voided_tax_documents PRIMARY KEY,
        tenant_id nvarchar(64) NOT NULL,
        document_type nvarchar(2) NOT NULL,
        establishment nvarchar(3) NOT NULL,
        emission_point nvarchar(3) NOT NULL,
        sequential nvarchar(9) NOT NULL,
        access_key nvarchar(49) NULL,
        authorization_number nvarchar(64) NULL,
        issue_date date NOT NULL,
        void_date date NOT NULL,
        fiscal_period nvarchar(7) NOT NULL,
        reason nvarchar(512) NOT NULL,
        source_document_id uniqueidentifier NULL,
        status nvarchar(32) NOT NULL,
        created_at_utc datetimeoffset NOT NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ux_voided_tax_documents_number' AND object_id = OBJECT_ID('financial.voided_tax_documents'))
    CREATE UNIQUE INDEX ux_voided_tax_documents_number ON financial.voided_tax_documents(tenant_id, document_type, establishment, emission_point, sequential);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ix_voided_tax_documents_period' AND object_id = OBJECT_ID('financial.voided_tax_documents'))
    CREATE INDEX ix_voided_tax_documents_period ON financial.voided_tax_documents(tenant_id, fiscal_period);
