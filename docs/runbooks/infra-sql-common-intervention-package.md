# Infra SQL Common Intervention Package

## Next Cycle P1 SQL control

SQL remediation must continue outside Financiero only with a named SQL Common / Infra Owner and DBA Owner. Current state remains `NoResponse` / `EvidencePending` / `BLOCKED_DEPENDENCY`; preflight remains `SCRIPT_EXIT=2`. Do not create SQL Server propio in Financiero.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 11 P5 SQL closure note

Sprint 11 closes without accepted SQL evidence. shared SQL TCP and `FinancieroDb` remain `NoResponse` / `EvidencePending` / `BLOCKED_DEPENDENCY`; P5 preflight returned `SCRIPT_EXIT=2` with shared SQL TCP closed. Do not create SQL Server propio in Financiero.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 11 P4 SQL follow-up note

No accepted SQL evidence was received in P4. shared SQL TCP and `FinancieroDb` remain `NoResponse` / `EvidencePending` / `BLOCKED_DEPENDENCY`; preflight returned `SCRIPT_EXIT=2` with shared SQL TCP closed. Do not create SQL Server propio in Financiero.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 11 P3 SQL escalation note

No accepted SQL evidence was received in P3. shared SQL TCP and `FinancieroDb` remain `NoResponse` / `EvidencePending` / `BLOCKED_DEPENDENCY`; preflight returned `SCRIPT_EXIT=2` with shared SQL TCP closed. Do not create SQL Server propio in Financiero.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 11 P2 SQL follow-up note

No accepted SQL evidence was received in P2. shared SQL TCP and `FinancieroDb` remain `EvidencePending` / `BLOCKED_DEPENDENCY`.

No SQL Server propio. No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 11 P1 SQL external remediation

SQL remediation must happen outside the Financiero repo. Required return evidence: shared SQL TCP PASS and separate `FinancieroDb` availability, both sanitized and accepted.

No SQL Server propio. No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 10 P5 SQL closure note

Sprint 10 closes as `BLOCKED_DEPENDENCY` with shared SQL and `FinancieroDb` evidence still pending. Sprint 11 Option A should remediate this outside the Financiero repo.

No SQL Server propio. No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 10 P4 SQL follow-up note

P4 follow-up confirms `NoResponse` / `EvidencePending` for shared SQL TCP and `FinancieroDb`. Productization remains blocked; do not create SQL Server propio in Financiero. Reopen PASS capture only with accepted SQL owner evidence.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready.

## Sprint 10 P3 escalation note

SQL Common TCP and `FinancieroDb` evidence remain `EvidencePending` / `BLOCKED_DEPENDENCY`. The required action is owner-provided sanitized evidence using the P3 formal request and escalation matrix; Financiero must not create SQL Server propio or share another domain database.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready.

Sprint 10 P2 review: SQL owner evidence was NotReceived, so Gate 1 and Gate 2 remain `BLOCKED_DEPENDENCY`.

Sprint 10 P1 requires SQL owners to use `docs/qa/templates/sql-common-evidence-template.md` and pass Gate 1/Gate 2 in `docs/qa/financial-sprint-10-p1-e2e-acceptance-gate.md`.

Sprint 9 P5 closure keeps this intervention as a Sprint 10 prerequisite. PASS requires owner evidence for shared SQL before Financiero runtime evidence can be accepted.

Sprint 9 P4 prepares the external SQL intervention needed to move Financiero from `BLOCKED_DEPENDENCY` to real E2E `PASS`.

This SQL package is required together with the Portal Gateway and Portal Shell intervention package; shared SQL alone is not enough for PASS.

## Observed problem

- `host.docker.internal` resolves from the local environment.
- TCP port `21433` remains closed.
- Financiero Docker Compose does not define its own SQL Server container and must keep using shared SQL.

## Expected target

- Host: `host.docker.internal`.
- Port: `21433`.
- Logical database: `FinancieroDb`.
- Shared SQL ownership: Portal/Infra, not Financiero.

## Current evidence

The latest preflight classifies shared SQL as `BLOCKED_DEPENDENCY` with `HOST_RESOLVES_BUT_PORT_CLOSED`.

## Host diagnostics

```powershell
Test-NetConnection -ComputerName host.docker.internal -Port 21433
docker ps --format "table {{.Names}}\t{{.Ports}}"
docker compose config
```

## Container diagnostics

```powershell
docker run --rm mcr.microsoft.com/mssql-tools /opt/mssql-tools/bin/sqlcmd -S host.docker.internal,21433 -Q "SELECT @@VERSION"
```

Use only approved non-secret credentials from the owner-controlled environment. Do not commit credentials or connection strings.

## Infra actions

- Start the shared SQL Server used by PortalCorporativo, Financiero, CRM and future domains.
- Expose the agreed TCP port `21433` to the host and Docker workloads.
- Validate host firewall rules.
- Validate that SQL accepts TCP connections.
- Validate that logical database `FinancieroDb` exists or can be initialized by approved migrations.
- Validate connectivity from Docker workloads to `host.docker.internal:21433`.

## Evidence expected for PASS

- TCP success for `host.docker.internal:21433`.
- Sanitized SQL connectivity proof with no credentials.
- Confirmation that `FinancieroDb` is a separate logical database.
- Confirmation that no SQL Server container was added to Financiero.
- Preflight exits `0` after Portal dependencies are also available.

## Do not do

- Do not create SQL Server inside Financiero.
- Do not share the same logical database across domains.
- Do not commit `.env`, passwords, connection strings or tokens.
- Do not upload real taxpayer XML or SRI responses.
- Do not enable SRI Production, official ATS, legal-final RIDE or productive XAdES.

Control tokens: BLOCKED_DEPENDENCY; shared SQL; not production-ready; No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
