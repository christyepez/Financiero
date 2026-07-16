# Sprint 7 P1 - Real Portal Shell Contract Hardening

## Objetivo

Endurecer la integración de Financiero con PortalCorporativo sin duplicar Security, Menu, Configuration ni Notification.

## Implementado

- Contrato Portal Shell versionado `1.0`.
- `source = portal | standalone`.
- `issuedAt`, `expiresAt`, `capabilities` y `warnings` sanitizados.
- Validación de contexto para modo `portal-integrated`.
- Bloqueo seguro en producción si falta Portal context o versión soportada.
- Menú filtrado por allow-list de rutas financieras, permisos y feature flags.
- Delegated auth opcional, en memoria, expiry-aware y solo desde Portal context.
- Telemetry/correlation preparada sin envío externo real.
- UX segura para contexto requerido o warnings de contrato.

## No implementado

- Login propio.
- OIDC/MSAL/Auth0 dentro de Financiero.
- Roles persistidos.
- Token storage.
- Menú productivo independiente.
- SRI/ATS/RIDE/XAdES productivo.

## Siguiente paso

Validar end-to-end con PortalCorporativo real e intercambiar contrato de contexto firmado por el equipo Portal.
