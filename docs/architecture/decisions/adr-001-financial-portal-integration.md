# ADR-001: Integración Financiero–PortalCorporativo

- Estado: Aceptado
- Fecha: 2026-07-06

## Contexto

Financiero requiere capacidades transversales ya provistas por PortalCorporativo y capacidades propias de contabilidad/SRI. Duplicarlas rompería ownership, seguridad e interoperabilidad.

## Decisión

1. PortalCorporativo es la plataforma transversal; Financiero es un dominio independiente.
2. La comunicación ocurre exclusivamente mediante APIs versionadas, adaptadores y eventos con correlationId.
3. Se prohíben bases/tablas compartidas y acceso directo a almacenamiento Portal.
4. Financiero reutiliza Gateway, health, logging y correlationId; extiende Security, Menu, Configuration y Workers; adapta Audit, Notification y Outbox/Inbox.
5. Content/File, Reporting, Integration productiva, IdP productivo y Angular Shell quedan BLOCKED hasta Portal Sprint 2; no se duplicarán localmente.
6. Sprint 1 empieza por Contabilidad Core: plan de cuentas, períodos, asientos, detalles y motor contable foundation, sin SRI productivo.
7. Firma electrónica, custodia de certificados, rotación, acceso y auditoría requieren ADR/threat model separados y permanecen diferidos.
8. Cada bounded context financiero posee persistencia y Outbox locales; consumidores Inbox son idempotentes. No se promete exactly-once.

## Consecuencias

Se preservan límites y reutilización, pero frontend final, documentos, reporting e integración productiva dependen del roadmap Portal. Contabilidad Core puede avanzar con contratos foundation actuales.

## Decisiones diferidas

Moneda base/multimoneda, jerarquía definitiva de cuentas, calendario fiscal, reversos, aprobaciones, numeración; contratos SRI, almacenamiento XML/RIDE, proveedor de firma, reporting, IdP y shell.
