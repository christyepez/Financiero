# Sprint 6 P3 - Portal Shell Contract Integration Foundation

Estado: implementado como contrato frontend reemplazable.

## Alcance

- Definir modelos `PortalShellContext`, usuario, tenant, permisos, menú, notificaciones, correlación, ambiente y feature flags.
- Separar modo `standalone` de `portal-integrated`.
- Mantener fallback standalone local seguro.
- Preparar contrato temporal `window.__PORTAL_SHELL_CONTEXT__`.
- Conectar autorización, menú, notificaciones, API base URL y correlation id al contexto Portal Shell.
- Agregar indicadores UX de modo shell, permisos y feature flags.

## No alcance

- Login propio.
- MSAL/Auth0/OIDC propio.
- Token storage.
- Roles propios persistidos.
- Portal Shell runtime real.
- Mutaciones productivas.
- SRI producción, XML completo, ATS oficial o RIDE legal final.

## Seguridad

- Los dev headers solo se permiten en standalone development y si `enableDevHeaders=true`.
- En `portal-integrated` no se inventan permisos.
- En production sin contexto Portal se muestra estado seguro y no se crea login alternativo.
- `allowMutations=false` y `allowXmlPreviewUi=false` permanecen forzados.

## Validación

- El frontend debe compilar y pasar `pnpm test`.
- El backend no se modifica.
- Docker y health checks deben mantenerse OK.
