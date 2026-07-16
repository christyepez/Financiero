# Portal Shell Readiness Matrix

| Capability | Status | Owner esperado | Financiero behavior | Gap |
| --- | --- | --- | --- | --- |
| PortalShellContext | READY | PortalCorporativo + Financiero | Contrato `1.0` con `contractVersion`, `source`, capabilities y warnings | Confirmar esquema final con Portal |
| User context | NEEDS_PORTAL_CONTRACT | PortalCorporativo | Usa usuario sintético en standalone | Identidad real delegada |
| Tenant context | NEEDS_PORTAL_CONTRACT | PortalCorporativo | Tenant default local | Multi-tenant real |
| Permissions | FOUNDATION_ONLY | Portal Security | Lee permisos delegados/contexto | Fuente productiva de permisos |
| Menu | FOUNDATION_ONLY | Portal Menu | Metadata local foundation | Publicación/consumo remoto |
| Feature flags | FOUNDATION_ONLY | Portal Configuration | Defaults seguros y forced-off production | Flags remotos versionados |
| Notifications | DEFERRED | Portal Notification | Placeholder local banner | Canal real Portal |
| Correlation | READY | Portal + Financiero | Header `X-Correlation-ID` | Alinear formato final |
| Telemetry | FOUNDATION_ONLY | Portal Observability | Adapter placeholder | Export real a observabilidad Portal |
| API base URL | READY | Portal Configuration | Configurable por environment/contexto | Gobierno de ambientes |
| Delegated auth | FOUNDATION_ONLY | Portal Security | Token delegado opcional en memoria, expiry-aware | OIDC/MSAL/Auth0 propio prohibido |
| Production missing context behavior | READY | Financiero | Bloquea dev headers/flags sensibles | Confirmar UX final con Portal |
| Standalone development mode | READY | Financiero | Modo local seguro | No usar como producción |
| Purchase/voided commands | FOUNDATION_ONLY | Financiero + Portal Security | Gated por flags/permisos | Permisos reales Portal |
| ATS/SRI official actions | BLOCKED | Portal + revisión fiscal | Forced-off | Revisión legal/tributaria |
| XML preview | BLOCKED | Financiero + Portal Content/File | Forced-off | Contrato seguro y revisión legal |

## Reglas

- Financiero no crea login ni roles.
- PortalCorporativo conserva ownership de Security, Menu, Configuration, Notification y Shell.
- Standalone es solo desarrollo.
- Producción sin contexto Portal debe fallar seguro.
