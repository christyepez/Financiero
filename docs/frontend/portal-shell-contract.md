# Portal Shell Contract

Este contrato es foundation y reemplazable. No implementa login propio ni asume MSAL/Auth0/OIDC.

## Contrato temporal

Portal puede inyectar contexto mediante:

```ts
window.__PORTAL_SHELL_CONTEXT__ = {
  contractVersion: '1.0',
  source: 'portal',
  issuedAt: '2026-07-16T00:00:00.000Z',
  expiresAt: '2026-07-16T01:00:00.000Z',
  capabilities: ['financial.shell.foundation'],
  warnings: [],
  user: {
    userId: 'portal-user-id',
    displayName: 'Usuario Portal',
    emailMasked: 'u***@example.com'
  },
  tenant: {
    tenantId: 'default',
    tenantName: 'Tenant'
  },
  permissions: {
    permissions: ['financial.electronicdocuments.read']
  },
  environment: {
    apiBaseUrl: 'http://localhost:8083',
    shellMode: 'portal-integrated',
    production: false
  },
  featureFlags: {
    showAtsXmlReadiness: true,
    showExternalApprovals: true,
    showPurchases: true,
    showVoidedDocuments: true,
    allowDevHeaders: false,
    allowPurchaseCommands: false,
    allowVoidedDocumentCommands: false,
    allowAtsOfficialActions: false,
    allowSriSubmission: false,
    allowXmlPreviewUi: false,
    allowMutations: false
  }
};
```

Angular también expone el `InjectionToken` `PORTAL_SHELL_CONTEXT`.

## Modos

- `standalone`: modo local seguro, usuario/tenant sintéticos, sin token real.
- `portal-integrated`: contexto delegado por Portal.

`standalone` solo es aceptable para desarrollo. En producción, si Portal no inyecta contexto real o la versión del contrato no está soportada, Financiero bloquea navegación sensible y muestra un mensaje seguro sin volcar el objeto de contexto.

## Seguridad

- No se guarda token en browser storage.
- `delegatedAuthToken` solo se usa en memoria durante la llamada HTTP.
- En production no se agregan `X-Dev-Permissions`.
- `allowMutations` y `allowXmlPreviewUi` se fuerzan a `false`.

Para P4, los comandos UI foundation solo se habilitan en development con `allowMutations=true` y el flag específico del comando. Producción permanece bloqueada.
- Emails, texto y metadata se sanitizan antes de usarse.
- `delegatedAuthToken` es opcional, se usa solo en memoria y no se acepta desde querystring, storage ni cookies propias.
- `contractVersion` soportado: `1.0`.
- Rutas de menú se filtran por allow-list financiera, permisos y feature flags.

## Readiness P5

La matriz operativa está en `docs/frontend/portal-shell-readiness-matrix.md`. Hasta que PortalCorporativo congele el contrato real:

- Financiero consume contexto de usuario/tenant/permisos como contrato delegado.
- El modo standalone es solo desarrollo seguro.
- En producción, la ausencia de contexto Portal debe bloquear capacidades sensibles.
- Menú, notificaciones, telemetría y auth delegada siguen bajo ownership de PortalCorporativo.

## Menú

Cada item define:

- `route`.
- `title`.
- `permission`.
- `featureFlag`.
- `icon`.
- `order`.
- `foundationOnly`.
- `readOnly`.

## Notificaciones

Standalone usa canal local `local-banner`. En `portal-integrated`, el adapter queda listo para delegar al Portal; no envía correos, Teams ni crea motor propio.
