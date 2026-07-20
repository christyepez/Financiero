## Sprint 11 P5 note

No Angular runtime changes were introduced in Sprint 11 P5. Sprint 11 closes as `BLOCKED_DEPENDENCY`; Portal Shell, PortalShellContext, Menu/permissions and correlation id evidence remain `NoResponse` / `EvidencePending`; P5 preflight returned `SCRIPT_EXIT=2`; standalone mode remains development-only and not production-ready.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 11 P4 note

No Angular runtime changes were introduced in Sprint 11 P4. Portal Shell, PortalShellContext, Menu/permissions and correlation id evidence remain `NoResponse` / `EvidencePending`; preflight returned `SCRIPT_EXIT=2`; standalone mode remains development-only and not production-ready.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 11 P3 note

No Angular runtime changes were introduced in Sprint 11 P3. Portal Shell, PortalShellContext, Menu/permissions and correlation id evidence remain `NoResponse` / `EvidencePending`; preflight returned `SCRIPT_EXIT=2`; standalone mode remains development-only and not production-ready.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

# Financial Angular Shell

## Sprint 11 P2 Portal evidence note

No Angular runtime behavior changes are introduced in Sprint 11 P2. Portal Shell and PortalShellContext evidence remains pending; standalone mode remains development-only and not production-ready.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## Sprint 11 P1 Portal Shell remediation note

No Angular runtime behavior changes are introduced in Sprint 11 P1. Portal Shell and PortalShellContext remediation must happen outside Financiero; standalone mode remains development-only.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready.

## Sprint 10 P5 closure UX note

Sprint 10 closure adds no Angular runtime behavior. Portal-integrated UX PASS remains blocked by missing Portal Shell, PortalShellContext, Menu/permissions and correlation id evidence. Standalone mode remains development-only.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready.

## Sprint 10 P4 Portal Shell executive block note

P4 adds no Angular runtime behavior. Portal Shell, PortalShellContext, Menu/permissions and correlation id remain `NoResponse` / `EvidencePending` / `BLOCKED_DEPENDENCY`; standalone mode remains development-only and cannot be used to claim Portal-integrated PASS.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready.

## Sprint 10 P3 Portal Shell note

P3 does not add Angular runtime features. Portal Shell, PortalShellContext, Menu/permissions and correlation id evidence remain `EvidencePending` / `BLOCKED_DEPENDENCY`; standalone mode remains development-only and cannot be used to claim Portal-integrated PASS.

No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES. Financiero remains not production-ready.

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

## Sprint 7 P3 Portal Content/File and Notification boundary

La pantalla muestra `/api/financial/external-approval-requests/integration-readiness`, marca evidencias como `reference only / Portal-owned evidence`, y mantiene disclaimers de `no upload` y `no notification send`. No existe input de archivo, descarga de evidencia ni control de envío de notificaciones desde Financiero.

## Sprint 7 P4 controlled productization readiness

Las pantallas de compras y anulados muestran blockers de productización, dependencias de approval foundation, Content/File y Notification, y mantienen visibles los límites `foundation only`. No renderizan botones de producción, envío SRI, ATS oficial, upload real ni notification send.

## Sprint 7 P5 closure posture

Sprint 7 P5 mantiene la UI como readiness/foundation. El verificador frontend exige documentos de cierre, flags productivos apagados, ausencia de token storage, ausencia de upload controls, ausencia de notification send controls y ausencia de controles de activación productiva.

## Sprint 8 P1 Portal E2E readiness

El dashboard muestra `Portal E2E readiness` consumiendo `/api/financial/portal-integration/readiness`. El panel muestra estado, modo `development standalone only`, recordatorio `production requires Portal context`, blockers y disclaimers sanitizados. No muestra token, claims completas, email completo, tenant raw sensible ni rutas fuera de allow-list.

## Sprint 8 P4 external approval UX hardening

La pantalla `ExternalApprovalsComponent` endurece la lectura funcional de aprobaciones externas:

- Mapea estados `Draft`, `Submitted`, `InReview`, `ApprovedFoundation`, `RejectedFoundation`, `Blocked`, `Superseded` y `Cancelled`.
- Muestra `ApprovedFoundation no habilita producción`.
- Muestra `Evidence reference is Portal-owned metadata only` y `No file stored in Financiero`.
- Muestra `Notification intent is prepared only; no send` y `Portal Notification owner`.
- Explica que `External approval does not replace legal/tax approval`.
- Explica que `Production requires Portal + legal/tax/security approval`.
- Mantiene upload, download, notification send, SRI producción, SRI Test real, ATS oficial, RIDE legal final y XAdES productivo bloqueados.

Los verificadores frontend fallan si estos límites desaparecen de la UX.

## Sprint 8 P5 closure posture

El frontend queda validado de forma estática/build, pero el cierre E2E completo sigue `BLOCKED_DEPENDENCY` hasta tener Portal Gateway/Shell real y SQL común. La UX no debe convertir dependency blockers en PASS visual ni ocultar que Financiero no está production-ready.

## Sprint 9 P1 runtime posture

La activación E2E real sigue bloqueada por infraestructura: no hay Portal Gateway/Shell live evidence. El shell Angular puede compilar y pasar verificadores, pero PASS runtime requiere `PortalShellContext` real con `source=portal`, menú/permisos/feature flags y correlation id.

## Sprint 9 P2 dependency diagnostics

El frontend no cambia runtime. Los verificadores exigen evidencia P2 y runbook de arranque para evitar que la UI o documentación conviertan `BLOCKED_DEPENDENCY` en PASS visual.

## Sprint 9 P3 PASS/BLOCKED evidence

El frontend sigue sin cambios runtime. La evidencia P3 y el checklist PASS exigen Portal Shell real antes de considerar la navegación integrada como validada.

El verificador `tools/verify-portal-e2e-contract.mjs` valida contrato `PortalShellContext`, flags seguros, rutas allow-listed, delegated auth en memoria, bloqueo de standalone en producción y ausencia de querystring tokens.

## Sprint 8 P2 E2E execution hardening

El dashboard muestra blockers resumidos de `Portal E2E readiness`, incluyendo “SQL común requerido” y “production requires Portal context”. No muestra tokens, claims raw ni tenant sensible. El script `tools/validate-portal-financiero-e2e.ps1` permite validar health/readiness de manera no invasiva cuando PortalCorporativo y SQL común están disponibles.

Sprint 8 P3 muestra clasificación clara: `PASS`, `BLOCKED DEPENDENCY`, `FAIL`, `FOUNDATION ONLY` según la respuesta y el script. La UI conserva comandos productivos apagados.

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
# Sprint 10 P2 Shell evidence review

No Portal Shell or PortalShellContext evidence was received. Financiero Angular remains static-verified only and cannot claim Portal-integrated PASS.

# Sprint 10 P1 Shell evidence

Portal Shell evidence must use `docs/qa/templates/portal-shell-evidence-template.md` and PortalShellContext evidence must use `docs/qa/templates/portal-contract-evidence-template.md`. Financiero still must not create a Shell or store tokens.

# Sprint 9 P5 closure note

Portal Shell remains externally owned and live evidence is still missing. Sprint 10 must confirm PortalShellContext, menu, permissions and correlation id before any production readiness claim.

# Sprint 9 P4 Portal Shell dependency

Portal Shell remains externally owned. P4 requires owner evidence for Shell URL, health route, PortalShellContext, menu, permissions and correlation id. Financiero must not create a Portal Shell, store tokens, enable upload/download evidence or send notifications.
