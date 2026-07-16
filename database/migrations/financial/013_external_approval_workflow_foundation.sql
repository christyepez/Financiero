IF SCHEMA_ID(N'financial') IS NULL
    EXEC(N'CREATE SCHEMA financial');

IF OBJECT_ID(N'financial.external_approval_requests', N'U') IS NULL
BEGIN
    CREATE TABLE financial.external_approval_requests
    (
        id UNIQUEIDENTIFIER NOT NULL CONSTRAINT pk_external_approval_requests PRIMARY KEY,
        tenant_id NVARCHAR(64) NOT NULL,
        scope NVARCHAR(64) NOT NULL,
        status NVARCHAR(64) NOT NULL,
        title NVARCHAR(256) NOT NULL,
        fiscal_period NVARCHAR(7) NULL,
        created_by_display_name NVARCHAR(256) NOT NULL,
        created_at_utc DATETIMEOFFSET NOT NULL,
        updated_at_utc DATETIMEOFFSET NOT NULL
    );
END

IF OBJECT_ID(N'financial.external_approval_requirements', N'U') IS NULL
BEGIN
    CREATE TABLE financial.external_approval_requirements
    (
        id UNIQUEIDENTIFIER NOT NULL CONSTRAINT pk_external_approval_requirements PRIMARY KEY,
        external_approval_request_id UNIQUEIDENTIFIER NOT NULL,
        tenant_id NVARCHAR(64) NOT NULL,
        description NVARCHAR(512) NOT NULL,
        requires_evidence BIT NOT NULL,
        requires_human_review BIT NOT NULL,
        created_at_utc DATETIMEOFFSET NOT NULL,
        CONSTRAINT fk_external_approval_requirements_request FOREIGN KEY (external_approval_request_id) REFERENCES financial.external_approval_requests(id) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'financial.external_approval_evidence_references', N'U') IS NULL
BEGIN
    CREATE TABLE financial.external_approval_evidence_references
    (
        id UNIQUEIDENTIFIER NOT NULL CONSTRAINT pk_external_approval_evidence_references PRIMARY KEY,
        external_approval_request_id UNIQUEIDENTIFIER NOT NULL,
        tenant_id NVARCHAR(64) NOT NULL,
        provider NVARCHAR(64) NOT NULL,
        reference_id NVARCHAR(256) NOT NULL,
        display_name NVARCHAR(256) NOT NULL,
        hash NVARCHAR(128) NULL,
        content_type NVARCHAR(128) NULL,
        created_at_utc DATETIMEOFFSET NOT NULL,
        created_by_display_name NVARCHAR(256) NULL,
        CONSTRAINT fk_external_approval_evidence_request FOREIGN KEY (external_approval_request_id) REFERENCES financial.external_approval_requests(id) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'financial.external_approval_decisions', N'U') IS NULL
BEGIN
    CREATE TABLE financial.external_approval_decisions
    (
        id UNIQUEIDENTIFIER NOT NULL CONSTRAINT pk_external_approval_decisions PRIMARY KEY,
        external_approval_request_id UNIQUEIDENTIFIER NOT NULL,
        tenant_id NVARCHAR(64) NOT NULL,
        decision_kind NVARCHAR(64) NOT NULL,
        reason NVARCHAR(512) NOT NULL,
        decided_by_display_name NVARCHAR(256) NOT NULL,
        decided_at_utc DATETIMEOFFSET NOT NULL,
        does_not_enable_production BIT NOT NULL,
        CONSTRAINT fk_external_approval_decisions_request FOREIGN KEY (external_approval_request_id) REFERENCES financial.external_approval_requests(id) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'financial.external_approval_timeline', N'U') IS NULL
BEGIN
    CREATE TABLE financial.external_approval_timeline
    (
        id UNIQUEIDENTIFIER NOT NULL CONSTRAINT pk_external_approval_timeline PRIMARY KEY,
        external_approval_request_id UNIQUEIDENTIFIER NOT NULL,
        tenant_id NVARCHAR(64) NOT NULL,
        action NVARCHAR(64) NOT NULL,
        message NVARCHAR(512) NOT NULL,
        created_at_utc DATETIMEOFFSET NOT NULL,
        actor_display_name NVARCHAR(256) NULL,
        CONSTRAINT fk_external_approval_timeline_request FOREIGN KEY (external_approval_request_id) REFERENCES financial.external_approval_requests(id) ON DELETE CASCADE
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_external_approval_requests_scope_status' AND object_id = OBJECT_ID(N'financial.external_approval_requests'))
    CREATE INDEX ix_external_approval_requests_scope_status ON financial.external_approval_requests(tenant_id, scope, status);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_external_approval_requests_period_created' AND object_id = OBJECT_ID(N'financial.external_approval_requests'))
    CREATE INDEX ix_external_approval_requests_period_created ON financial.external_approval_requests(tenant_id, fiscal_period, created_at_utc);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'ix_external_approval_evidence_request' AND object_id = OBJECT_ID(N'financial.external_approval_evidence_references'))
    CREATE INDEX ix_external_approval_evidence_request ON financial.external_approval_evidence_references(tenant_id, external_approval_request_id, created_at_utc);
