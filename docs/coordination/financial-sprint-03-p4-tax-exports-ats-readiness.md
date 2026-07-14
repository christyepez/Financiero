# Sprint 3 P4 - Tax Exports / ATS Readiness / Advanced Reporting

Status: implemented as foundation.

Scope delivered:

- In-memory tax export foundation for JSON and CSV.
- ATS readiness foundation as internal evaluation only.
- Advanced tax reporting endpoints for action queue and monthly summary.
- Audit-only metadata for export/readiness/reporting queries.
- Smoke flow extended for exports, action queue, monthly summary and ATS readiness.

Out of scope:

- Official ATS XML generation.
- ATS submission.
- SRI production or real SRI Test send.
- Real certificates, XAdES production and frontend dashboards.
- Productive file storage or Portal Content/File payload upload.

Security:

- Access keys and customer/subject identifications are masked by default.
- Exports do not include XML payloads, secrets, certificates or private keys.
- Export files are generated in memory and are not written to the filesystem.
- `includeSensitive` remains false by default and is documented as an internal-only flag.

Validation evidence:

- `dotnet build Financiero.sln` passes.
- `DOTNET_ROLL_FORWARD=Major dotnet test Financiero.sln` passes.
- Direct `dotnet test` is blocked by missing local .NET 8 runtime.
