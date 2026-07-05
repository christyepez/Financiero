# Codex Instructions - Financiero

## Contexto

El repositorio Financiero contiene la arquitectura, backlog, agentes y contratos para implementar el módulo financiero/contable/tributario/SRI.

El repositorio de portal reutilizable es:

```text
https://github.com/christyepez/PortalCorporativo
```

## Regla principal

No construir una aplicación financiera independiente. Construir un módulo financiero nativo del portal corporativo.

## Flujo obligatorio

1. Revisar `AGENTS.md`.
2. Revisar `docs/accounting-tax-platform/18-codex-agent-coordinator.md`.
3. Ejecutar Sprint 0.
4. Inspeccionar PortalCorporativo.
5. Generar inventario real de componentes reutilizables.
6. Clasificar cada componente como REUSE, EXTEND, ADAPT, CREATE o BLOCKED.
7. Solo después crear código.

## Componentes del portal a buscar

Codex debe inspeccionar en PortalCorporativo:

```text
backend
frontend/portal-angular
codex
configs
docs
infrastructure/sql
docker-compose.yml
AGENTS.md
README.md
```

Buscar específicamente:

- Security API.
- Menu API.
- Integration API.
- Reporting API.
- Audit API.
- Notification API.
- Content API.
- Catalog API.
- Configuration API.
- SQL Outbox.
- Workers.
- Docker Compose.
- Angular shell.
- Guards.
- Grids configurables.
- Menús configurables.
- Parámetros visuales.

## Criterio de implementación

Crear código únicamente después de documentar la decisión de reutilización.

## Comando de arranque sugerido

```text
Ejecuta Sprint 0. No implementes dominio financiero todavía. Primero inspecciona PortalCorporativo y genera los documentos reales de reutilización.
```
