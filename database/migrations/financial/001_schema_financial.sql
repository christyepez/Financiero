IF SCHEMA_ID('financial') IS NULL EXEC('CREATE SCHEMA financial');

IF OBJECT_ID('financial.schema_versions', 'U') IS NULL
BEGIN
    CREATE TABLE financial.schema_versions (
        Version nvarchar(32) NOT NULL PRIMARY KEY,
        ScriptName nvarchar(256) NOT NULL,
        AppliedAtUtc datetimeoffset NOT NULL
    );
END;
