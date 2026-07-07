# Instrucciones para agentes

Leer primero `README.md`, `codex/PROJECT_CONTEXT.md`, `codex/PORTAL_INTEGRATION_CONTRACTS.md`, `codex/TASKS.md` y los ADR aplicables. No leer todo el repositorio.

Antes de crear componentes, clasificar REUSE, EXTEND, ADAPT, CREATE o BLOCKED. PortalCorporativo conserva identidad, Gateway, Menu, Configuration, Audit, Notification y capacidades transversales. Financiero conserva únicamente contabilidad, tributación y SRI.

Reglas: no compartir bases con Portal, no guardar secretos/certificados, autorización backend, Outbox en la transacción local, correlationId obligatorio, contratos versionados y pruebas de invariantes. Mantener cambios pequeños; ejecutar build/tests y reportar impacto Portal.
