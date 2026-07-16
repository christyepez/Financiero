# Sprint 6 P2 - Angular Real Data Wiring

Estado: implementado como integración read-only sanitizada contra backend local.

## Alcance

- Desempaquetar `ApiResponse<T>` en el cliente Angular.
- Conectar dashboard y pantallas foundation a endpoints reales existentes.
- Agregar estados loading/error/empty.
- Agregar selector de período para ATS, compras y anulados.
- Mantener adapters Portal como placeholders reemplazables.
- Mantener UI sin mutaciones productivas.

## Endpoints consumidos

- `/api/financial/electronic-documents/sri/readiness`
- `/api/financial/electronic-documents/content-file/readiness`
- `/api/financial/tax-reporting/ats-readiness`
- `/api/financial/tax-reporting/ats-section-readiness`
- `/api/financial/tax-reporting/ats-xml/readiness`
- `/api/financial/external-approvals`
- `/api/financial/external-approvals/readiness?scope=all`
- `/api/financial/tax-catalogs`
- `/api/financial/purchases?period=YYYY-MM`
- `/api/financial/voided-documents?period=YYYY-MM`

## Seguridad

- No login propio.
- No localStorage/sessionStorage.
- `X-Dev-Permissions` queda bloqueado en production aunque se configure por error.
- Sanitización defensiva para identificaciones y access keys si el backend retorna campos no enmascarados.
- No XML completo en UI.

## Limitaciones

- Las aprobaciones externas siguen advisory/read-only.
- No se generan previews XML ATS en P2.
- Portal Shell real sigue pendiente hasta contrato congelado.
