# Financial Sprint 4 QA Evidence

## Validation summary

- Last reported test total: 203 passed.
- `dotnet restore`: passed in P4.
- `dotnet build`: passed in P4.
- `dotnet test`: passed in P4.
- `DOTNET_ROLL_FORWARD=Major dotnet test`: passed in P4.
- `docker compose config`: passed and confirmed shared SQL connection shape.

## Docker runtime

`docker compose up -d --build` was attempted during Sprint 4 packages and was blocked by external MCR timeout resolving `mcr.microsoft.com/dotnet/aspnet:8.0`.

Health and smoke are not declared OK when Docker did not start.

## SQL validation

Financiero does not define a SQL Server container. It points to the shared local SQL host and uses logical database `FinancieroDb`.

## Secret/certificate/XML scan

Scans found no real certificate files intended for commit. `.env` is local and not staged. XML/certificate keywords appear only in tests, sanitizers or smoke checks.

## Pending QA risks

- Re-run Docker runtime once MCR is available.
- Execute health endpoints and smoke scripts after runtime starts.
- Capture external review evidence before official enablement.
