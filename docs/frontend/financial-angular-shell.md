# Financial Angular Shell

`frontend/financiero-web` es la primera base Angular de Financiero y actúa como consumidor del Portal, no como plataforma paralela.

## Integración Portal

- `PortalAuthAdapter`: punto de conexión para token/permisos del Portal. No guarda tokens en browser storage.
- `PortalMenuAdapter`: metadata local foundation para futura publicación/consumo desde Portal Menu API.
- `PortalNotificationAdapter`: placeholder para estados de UX; no implementa proveedor propio.

## Seguridad

- `apiAuthorizationInterceptor` agrega `Authorization` solo si el Portal provee token.
- `X-Dev-Permissions` solo se agrega si `enableDevHeaders=true`.
- `correlationIdInterceptor` agrega `X-Correlation-ID`.
- `errorSanitizationInterceptor` evita mostrar XML, certificados, tokens, secretos o campos sensibles.

## Vistas

- Dashboard.
- SRI readiness.
- ATS readiness.
- External approvals.
- Tax catalogs.
- Purchases read-only.
- Voided documents read-only.

## Sprint 6 P2 real data wiring

Las vistas consumen endpoints reales existentes y desempaquetan `ApiResponse<T>` desde `ApiService`. Los componentes muestran loading/error/empty, usan selector de período donde aplica y mantienen los datos en modo read-only.

Rutas consumidas:

- `/api/financial/electronic-documents/sri/readiness`.
- `/api/financial/electronic-documents/content-file/readiness`.
- `/api/financial/tax-reporting/ats-readiness`.
- `/api/financial/tax-reporting/ats-section-readiness`.
- `/api/financial/tax-reporting/ats-xml/readiness`.
- `/api/financial/external-approvals`.
- `/api/financial/external-approvals/readiness?scope=all`.
- `/api/financial/tax-catalogs`.
- `/api/financial/purchases?period=YYYY-MM`.
- `/api/financial/voided-documents?period=YYYY-MM`.

La UI no muestra XML completo, access keys completas, identificaciones completas ni secretos. `X-Dev-Permissions` solo puede activarse en development y permanece apagado por defecto.

## Sprint 6 P3 Portal Shell contract

Se agrega una capa reemplazable para integrar el shell financiero con PortalCorporativo:

- `PortalShellContext` como contrato de usuario, tenant, permisos, menú, notificaciones, correlación, ambiente y feature flags.
- `StandalonePortalContextProvider` para ejecución local segura.
- `PortalIntegratedContextProvider` para contexto futuro desde Portal.
- `PortalContextAdapter` como fachada para auth/menu/configuración/notificaciones.
- Contrato temporal `window.__PORTAL_SHELL_CONTEXT__`, documentado en `docs/frontend/portal-shell-contract.md`.

El frontend mantiene `standalone` como fallback local y `portal-integrated` como modo preparado. No implementa login propio, roles propios, token storage ni runtime real de Portal.

## Sprint 6 P4 controlled UI commands

Se agregan comandos foundation controlados para compras y documentos anulados:

- Crear compra foundation.
- Validar compra foundation.
- Registrar documento anulado foundation.

Los comandos requieren `financial.electronicdocuments.manage` y flags explícitos:

- `allowMutations=true`.
- `allowPurchaseCommands=true` para compras.
- `allowVoidedDocumentCommands=true` para anulados.

Por defecto todos permanecen apagados. `allowAtsOfficialActions`, `allowSriSubmission` y `allowXmlPreviewUi` permanecen `false`.

## Sprint 6 P5 closure and UX hardening

P5 cierra Sprint 6 como foundation de UX/Portal readiness. Las pantallas refuerzan:

- Disclaimers visibles `Foundation / No productivo`.
- Badges para estados read-only, gated, no oficial y producción bloqueada.
- Mensajes de error sanitizados.
- Empty states con guía para validar período, API local y permisos Portal.
- Comandos apagados por defecto y dependientes de contexto Portal.

No se agregan mutaciones nuevas, login propio, token storage, SRI Test real, producción SRI, ATS oficial ni preview XML completo.

## Sprint 7 P1 real Portal Shell contract hardening

El contrato Portal Shell se endurece para integración real:

- `contractVersion` soportado: `1.0`.
- `source`: `portal` o `standalone`.
- `capabilities` y `warnings` sanitizados.
- Validación de contexto requerido en `portal-integrated`.
- Bloqueo seguro en producción si falta Portal context o versión soportada.
- Menú filtrado por rutas permitidas, permisos y feature flags.
- Delegated auth solo en memoria y solo desde contexto Portal.
- Telemetría/correlation preparada sin export externo real.

Financiero sigue sin login propio, roles propios, token storage, cookies auth propias ni duplicación de Security/Menu/Configuration/Notification del Portal.

## Sprint 7 P2 external approval persistence

La pantalla de aprobaciones externas consume `/api/financial/external-approval-requests` para listar requests persistidos foundation. Los comandos de crear, submit, review, referencia de evidencia, decisión y cancelación requieren:

- `allowMutations=true`.
- `allowExternalApprovalCommands=true`.
- `financial.electronicdocuments.manage`.

Referencia de evidencia es metadata únicamente; no hay upload, XML preview, certificados ni archivos.

## Configuración

Copiar `src/environments/environment.example.ts` como referencia. Los valores por defecto apuntan a `http://localhost:8083` y mantienen los headers dev apagados.

## Ejecución

```powershell
cd frontend/financiero-web
npm install
npm run build
npm test
```
