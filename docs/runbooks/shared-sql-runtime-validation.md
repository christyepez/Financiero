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

## Validate database

Use an approved local SQL client outside the repo. Record only:

- server reachable: yes/no;
- database `FinancieroDb` exists: yes/no;
- migrations initialized: yes/no.

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
