# Shared SQL Runtime Validation

## Goal

Validate the single shared SQL Server used by PortalCorporativo, Financiero, CRM and future domains. Each domain must use a separate logical database, not a separate SQL container.

## Expected local model

- One SQL Server container/runtime only.
- `PortalCorporativoDb` for Portal.
- `FinancieroDb` for Financiero.
- `CrmDb` for CRM when present.

## Validate host connectivity

```powershell
Test-NetConnection host.docker.internal -Port 21433
```

Expected: TCP test succeeds. If it fails, do not treat Financiero health as failed application logic; the shared SQL runtime is unavailable.

Script equivalent:

```powershell
tools/validate-portal-financiero-e2e.ps1 -SkipPortalChecks -SkipApiHealthChecks -OutputMarkdown
```

Exit code `2` means `BLOCKED_DEPENDENCY`, not an application failure.

Sprint 8 P5 closure keeps final E2E status as `BLOCKED_DEPENDENCY` while shared SQL is unavailable. Do not create a Financiero-owned SQL Server to turn this into PASS.

Sprint 9 P1 expected PASS evidence:

- DNS resolves `host.docker.internal`.
- TCP succeeds on port `21433`.
- `docker compose config` contains no SQL Server service in Financiero.
- Financiero uses logical database `FinancieroDb`.
- Portal/infra shared runtime owns the single SQL Server container.

Sprint 9 P1 expected BLOCKED_DEPENDENCY evidence:

- DNS resolves but TCP fails on `21433`.
- SQL container is stopped or port mapping is missing.
- Financiero API readiness fails because SQL is unavailable.
- No SQL Server is added to Financiero to bypass the dependency.

Sprint 9 P2 diagnostic codes:

| Code | Meaning | Action |
|---|---|---|
| `HOST_NOT_RESOLVED` | SQL host alias cannot resolve. | Validate Docker Desktop networking or override `-SqlHost`. |
| `HOST_RESOLVES_BUT_PORT_CLOSED` | Host resolves but TCP port is closed. | Start shared SQL, validate `-SqlPort`, port mapping and firewall. |
| `PASS` | Check succeeded. | Continue to next dependency. |

Use overrides when the shared SQL runtime intentionally uses different host/port:

```powershell
.\tools\validate-portal-financiero-e2e.ps1 -SqlHost <host> -SqlPort <port> -SkipPortalChecks -SkipApiHealthChecks -OutputMarkdown -VerboseDiagnostics -SuggestFixes
```

Sprint 9 P3 status: DNS resolves but port `21433` remains closed. The next action is external: start shared SQL or provide a correct host/port override.

## Validate from Financiero compose

```powershell
docker compose config
```

Confirm:

- no `mcr.microsoft.com/mssql` image;
- no `1433:1433` SQL port mapping;
- `ConnectionStrings__FinancialDb` points to the common SQL host/port;
- database name is `FinancieroDb`.

Do not print or commit real connection strings.

## Validate from container

Start the API only after shared SQL is running:

```powershell
docker compose up -d --build financial-api
docker compose logs --tail 80 financial-api
```

If logs show `Could not open a connection to SQL Server`, validate shared SQL first.

Container-side validation after shared SQL is up:

```powershell
docker compose up -d --build financial-api
docker compose ps
docker compose logs --tail 80 financial-api
```

Record only health state and host/port. Do not copy credentials.

## Validate database

Use an approved local SQL client outside the repo. Record only:

- server reachable: yes/no;
- database `FinancieroDb` exists: yes/no;
- migrations initialized: yes/no.

Database validation PASS requires database name `FinancieroDb`, separate from `PortalCorporativoDb` and any `CrmDb`.

Do not copy passwords, connection strings, dumps or customer data into evidence.

## Diagnose timeout

- Confirm the shared SQL container is running.
- Confirm port mapping uses the expected host port.
- Confirm firewall allows local TCP traffic.
- Confirm Docker can resolve `host.docker.internal`.
- Confirm credentials are provided from local untracked environment.

## Align Portal and Financiero

- PortalCorporativo and Financiero must target the same SQL Server host/port.
- They must use different logical databases.
- Do not add SQL Server service to Financiero compose.

## Evidence format

Record sanitized facts only:

- command name;
- PASS / BLOCKED_DEPENDENCY / FAIL;
- host and port without credentials;
- database names;
- timestamp;
- blocker summary.
# Sprint 9 P4 SQL intervention

Current P4 evidence requires Infra to prove shared SQL TCP connectivity on `host.docker.internal:21433` and logical database `FinancieroDb`. Do not create a SQL Server service in Financiero. See `docs/runbooks/infra-sql-common-intervention-package.md`.

Control tokens: BLOCKED_DEPENDENCY; shared SQL; not production-ready; No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
