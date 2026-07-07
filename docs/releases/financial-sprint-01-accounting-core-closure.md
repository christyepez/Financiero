# Financiero Sprint 1 — Accounting Core Closure

## Objetivo

Cerrar formalmente Sprint 1 Accounting Core de Financiero como dominio consumidor de PortalCorporativo, con contabilidad base operativa, seguridad runtime, QA integrada y preparación para Sprint 2 sin duplicar capacidades transversales del Portal.

## Alcance entregado

- Bootstrap .NET 8 con Clean Architecture.
- Base lógica `FinancieroDb` en SQL Server común de PortalCorporativo.
- Plan de cuentas.
- Años fiscales.
- Períodos fiscales.
- Asientos contables con líneas, contabilización, reverso y void de borradores.
- Autorización runtime por permisos `financial.*`.
- Adaptadores de desarrollo para Audit, Outbox, Configuration y Notification.
- Migraciones SQL versionadas y runner básico.
- Health/readiness con validación de tablas core.
- QA integrado, security smoke y smoke Docker.

## Fuera de alcance

- SRI, facturación electrónica, retenciones, notas de crédito/débito, liquidaciones y guías.
- Frontend Angular.
- Reporting financiero/fiscal.
- Portal HTTP adapters productivos.
- Workers productivos.
- Migraciones productivas con herramienta formal externa.

## PRs mergeados

- PR #3 — P1 Chart of Accounts.
- PR #4 — P2 Fiscal Periods.
- PR #5 — P3 Journal Entries.
- PR #6 — P4 Portal Adapters / Runtime Security / Accounting Hardening.
- PR #7 — P5 Integrated QA / SQL Concurrency / Security Smoke / Migration Versioning.

## Commits principales

- P5 merge commit usado como base de cierre: `48e3fb96647e0a44c1d5d24335b61537bb801d40`.

## Arquitectura final Sprint 1

Financiero queda como modular monolith backend con módulos Accounts, Fiscal Years, Fiscal Periods y Journal Entries. Usa esquema SQL `financial` dentro de `FinancieroDb`. PortalCorporativo conserva ownership de Security, Gateway, Configuration, Audit, Notification, logging/correlation y SQL Server local común.

## Capacidades funcionales entregadas

- Administración de cuentas contables.
- Administración de años y períodos fiscales.
- Registro de asientos contables.
- Validación de partida doble al contabilizar.
- Numeración de asientos por tenant/año con locks SQL.
- Reverso de asientos contabilizados.
- Void de asientos en borrador.

## Capacidades Portal reutilizadas

- Gateway como entrada esperada.
- Modelo de permisos Security.
- Puertos Configuration, Audit y Outbox.
- Health/logging/correlationId.
- SQL Server común local.

## Validaciones ejecutadas

- `dotnet build Financiero.sln` — OK.
- `dotnet test Financiero.sln --no-build` — OK, 61/61.
- `docker compose config --quiet` — OK.
- `/health/ready` — 200.
- `scripts/smoke/financial-smoke.ps1 -BaseUrl http://localhost:8083` — OK.

## Estado Docker

`financial-api` queda ejecutable por Docker Compose. El puerto del host es configurable con `FINANCIAL_API_PORT`; el ambiente validado de P5 usó `8083`.

## Estado SQL común

Financiero no define contenedor SQL Server propio. Usa el SQL Server común de PortalCorporativo, expuesto localmente en `21433`, con base lógica separada `FinancieroDb`.

## Estado de seguridad runtime

Los endpoints financieros aplican permisos específicos. En Development se admite `X-Dev-Permissions` para smoke; en Production los permisos deben venir de claims JWT emitidos por Portal/IdP.

## Estado de migraciones

Existen scripts versionados en `database/migrations/financial` y runner básico con historial en `financial.schema_versions`. La herramienta formal de migraciones queda como deuda controlada.

## Estado de QA

Sprint 1 cuenta con tests de dominio, aplicación, API, flujo integrado, security smoke y smoke Docker.

## Riesgos abiertos

- Integración HTTP productiva con Portal pendiente de contratos cerrados.
- Pruebas extendidas de carga/concurrencia aún pendientes.
- Custodia de certificados y secretos SRI pendiente.
- Frontend y reporting dependen de capacidades Portal Sprint 2.

## Deudas controladas

- Reemplazar o robustecer runner básico de migraciones.
- Añadir prueba de concurrencia SQL extendida.
- Implementar adapters HTTP productivos hacia Portal.
- Diseñar almacenamiento de XML/PDF tributario con Content/File Portal.

## Criterios de aceptación cumplidos

- No se duplicó SQL Server.
- No se compartieron tablas con Portal.
- No se implementaron capacidades SRI ni frontend.
- Build, tests y smoke documentados como OK.
- Repositorio listo para Sprint 2.

## Decisión de cierre

Sprint 1 Accounting Core queda cerrado y listo para iniciar Sprint 2.
