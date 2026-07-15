# Sprint 5 P2 - ATS Support Document Mapping Hardening

Estado: implementado como hardening foundation. No genera XML ATS oficial, no certifica cumplimiento tributario y no activa integración SRI productiva.

## Alcance implementado

- Catálogo read-only de reglas de mapeo entre documentos foundation y secciones ATS.
- Mapeo foundation para compras, notas de crédito, notas de débito, retenciones relacionadas, liquidación de compra placeholder y anulados.
- Endpoints protegidos para consultar mapeo ATS por compra y documento anulado.
- Endpoint de readiness por sección ATS para compras y anulados.
- Integración de readiness/gaps ATS con detalle de campos faltantes, secciones unsupported y disclaimer tributario.
- Smoke SRI mock extendido para validar mapping/readiness y sanitización.

## Fuera de alcance

- XML ATS oficial.
- Layout/XSD oficial.
- Envío SRI producción.
- Catálogos tributarios oficiales finales.
- Almacenamiento documental propio.
- Frontend Angular.

## Seguridad y Portal reuse

- Permiso usado: `financial.electronicdocuments.read`.
- Auditoría delegada a Portal Audit mediante eventos de consulta.
- No se crean tablas ni migración `013`.
- No se exponen XML, certificados, claves privadas, access key completa ni identificación completa.
- SQL Server común local se mantiene; Financiero usa únicamente la base lógica `FinancieroDb`.

## QA

- Tests application cubren mapeos foundation, faltantes, anulados, readiness por sección y gaps ATS.
- Tests API cubren permisos positivos/negativos e input inválido.
- Smoke valida endpoints nuevos y ausencia de payload sensible.
