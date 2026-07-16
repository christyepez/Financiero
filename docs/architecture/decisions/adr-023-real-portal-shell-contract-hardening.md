# ADR-023 - Real Portal Shell Contract Hardening

## Status

Accepted.

## Context

Financiero consume PortalCorporativo como plataforma transversal. El Sprint 6 creó un contrato foundation; Sprint 7 P1 lo endurece para evitar fallbacks inseguros y contratos ambiguos.

## Decision

- Soportar contrato `PortalShellContext` versión `1.0`.
- Exigir contexto Portal real en producción.
- Mantener standalone solo para desarrollo.
- Usar delegated auth únicamente en memoria.
- Filtrar menú por rutas permitidas, permisos y feature flags.
- Forzar flags peligrosos apagados en producción.
- Preparar telemetry/correlation sanitizada sin proveedor externo propio.

## Consequences

- Financiero no duplica login, roles, menú ni configuración.
- PortalCorporativo conserva ownership de Shell/Security/Menu/Configuration/Notification.
- Producción falla seguro si Portal no inyecta contexto válido.
- La integración real con Portal debe validar contrato end-to-end en Sprint 7.
