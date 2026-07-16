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

## Configuración

Copiar `src/environments/environment.example.ts` como referencia. Los valores por defecto apuntan a `http://localhost:8083` y mantienen los headers dev apagados.

## Ejecución

```powershell
cd frontend/financiero-web
npm install
npm run build
npm test
```
