# ADR-021 - Portal Shell Contract Integration Foundation

## Estado

Aprobado para Sprint 6 P3.

## Contexto

Financiero necesita integrarse con el Angular Shell de PortalCorporativo sin duplicar login, seguridad, menú, notificaciones ni auditoría. El contrato final de Portal aún puede evolucionar.

## Decisión

Crear una capa frontend reemplazable basada en `PortalShellContext`:

- `StandalonePortalContextProvider` para desarrollo local.
- `PortalIntegratedContextProvider` para contexto externo futuro.
- `PortalContextAdapter` como fachada estable para el resto del frontend.
- Contrato temporal `window.__PORTAL_SHELL_CONTEXT__` y `PORTAL_SHELL_CONTEXT` como alternativas de integración.

## Consecuencias

- El shell financiero sigue funcionando localmente.
- La integración real con Portal podrá reemplazar el provider sin reescribir pantallas.
- No se agrega identidad propia ni almacenamiento de token.

## Reglas

- Dev headers bloqueados en production.
- Mutaciones y XML preview UI bloqueados por defecto.
- El menú financiero se expresa como metadata read-only/foundation.
- Las notificaciones se delegan al Portal cuando exista runtime real.
