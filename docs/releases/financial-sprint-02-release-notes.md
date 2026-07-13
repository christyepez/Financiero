# Sprint 2 Release Notes - SRI & Electronic Invoicing Readiness

## Audiencia

Stakeholders técnicos, líderes funcionales, QA, DevOps y Security.

## Entregado

- Readiness SRI para factura electrónica Ecuador.
- Foundation de factura electrónica y documentos tributarios.
- Generación y validación XML base.
- Clave de acceso SRI con módulo 11.
- Estrategia de firma y readiness XAdES.
- Secret Store readiness.
- SRI Test dry-run y probe manual.
- Portal Content/File readiness.
- RIDE/PDF foundation.
- Observabilidad y sanitización.

## Controles de seguridad

- Producción bloqueada.
- No certificados reales en repo.
- No secretos reales en repo.
- No XML reales en repo.
- Logs y respuestas sanitizadas.
- Permisos `financial.electronicdocuments.*` preservados.

## Limitaciones conocidas

- XAdES real pendiente.
- SRI Test real send pendiente.
- Portal Content/File productivo pendiente.
- RIDE final pendiente.
- NC/ND/Retenciones/Guías/ATS pendientes.

## Impacto funcional

Sprint 2 no habilita operación productiva SRI; deja preparada la base técnica segura para validación manual y futuros documentos tributarios.
