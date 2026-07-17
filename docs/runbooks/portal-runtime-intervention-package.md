# Portal Runtime Intervention Package

Sprint 9 P4 prepares the Portal Gateway and Portal Shell intervention needed before Financiero can claim real E2E `PASS`.

## Observed problem

- Portal Gateway base URL responds, but `/health` returned HTTP 404.
- Portal Shell live evidence is not available.
- Financiero must not create a duplicate Gateway or Shell.

## Gateway health routes to confirm

Portal owner must confirm one documented route:

- `/health`
- `/health/live`
- `/health/ready`
- `/api/health`
- another Portal-documented route

If a non-default path is used, run the preflight with `-PortalGatewayHealthPath`.

## Runtime URLs to confirm

- Portal Gateway base URL, expected local default: `http://localhost:8082`.
- Portal Shell base URL, expected local default: `http://localhost:4201` or Portal-provided URL.
- Portal Shell health path, expected local default: `/health`.

## PortalShellContext contract evidence

Portal owner must provide sanitized evidence for:

- `contractVersion=1.0`.
- `source=portal`.
- delegated permissions including `financial.electronicdocuments.read`.
- menu routes compatible with Financiero allow-list.
- safe feature flags with productive actions disabled.
- `correlationId` propagation.

## Evidence expected for PASS

- Gateway health returns HTTP 2xx at the agreed route.
- Shell health/base route returns HTTP 2xx.
- PortalShellContext is available and sanitized.
- Financiero preflight exits `0` after shared SQL is available.

## Do not do

- Do not create Gateway inside Financiero.
- Do not create Shell inside Financiero.
- Do not duplicate Security, Menu, Configuration, Audit, Outbox, Notification or Content/File.
- Do not expose or commit tokens.
- Do not enable SRI Production, official ATS, legal-final RIDE or productive XAdES.

Control tokens: BLOCKED_DEPENDENCY; Portal Gateway; shared SQL; not production-ready; No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.
