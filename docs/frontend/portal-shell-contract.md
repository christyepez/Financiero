# Portal Shell Contract

Este contrato es foundation y reemplazable. No implementa login propio ni asume MSAL/Auth0/OIDC.

## Contrato temporal

Portal puede inyectar contexto mediante:

```ts
window.__PORTAL_SHELL_CONTEXT__ = {
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
    allowXmlPreviewUi: false,
    allowMutations: false
  }
};
```

Angular también expone el `InjectionToken` `PORTAL_SHELL_CONTEXT`.

## Modos

- `standalone`: modo local seguro, usuario/tenant sintéticos, sin token real.
- `portal-integrated`: contexto delegado por Portal.

## Seguridad

- No se guarda token en browser storage.
- `delegatedAuthToken` solo se usa en memoria durante la llamada HTTP.
- En production no se agregan `X-Dev-Permissions`.
- `allowMutations` y `allowXmlPreviewUi` se fuerzan a `false`.
- Emails, texto y metadata se sanitizan antes de usarse.

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
