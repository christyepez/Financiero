# Financial Sprint 6 Architecture Snapshot

## Frontend

`frontend/financiero-web` es Angular standalone y consume el backend Financiero mediante clientes API tipados. No implementa plataforma paralela.

## Portal integration

- `PortalShellContext`: contrato temporal para usuario, tenant, permisos, menú, flags, environment, correlation y notificaciones.
- `StandalonePortalContextProvider`: modo local seguro.
- `PortalIntegratedContextProvider`: placeholder para contexto real del Portal.
- Adapters de auth/menu/configuración/notificaciones quedan reemplazables.

## Security

- No login propio.
- No roles persistidos.
- No token storage.
- Authorization delegated token solo en memoria si Portal lo provee.
- `X-Dev-Permissions` bloqueado en production.
- Feature flags sensibles forced-off en production.

## UX

Todas las vistas son foundation/no productivo. P5 agrega badges y mensajes consistentes para readiness, bloqueos, errores y empty states.

## Backend

Sin cambios backend en P5. La API existente mantiene health/readiness, Audit/Outbox, permisos runtime y base lógica `FinancieroDb`.

## Docker / SQL

Financiero no define SQL Server propio. Usa SQL Server común local y base lógica separada.
