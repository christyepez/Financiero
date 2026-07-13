# Sprint 3 P2 - SRI Catalogs / Tax Rules Hardening

## Objetivo

Endurecer catálogos internos y reglas tributarias foundation para Factura, Nota de Crédito, Nota de Débito y Retenciones sin activar SRI real, XAdES productivo, frontend ni storage documental propio.

## Alcance implementado

- Catálogo interno foundation versionable: `2026-07-sprint-3-p2-foundation`.
- Provider `DevelopmentSriCatalogProvider`.
- Validación de tipos de documento `01`, `04`, `05`, `07`.
- Validación de tipos de identificación foundation.
- Validación de ambiente y tipo de emisión normal.
- Validación de documentos relacionados/sustento con formato `###-###-#########`.
- Validación de periodo fiscal `MM/YYYY` o `YYYY-MM`.
- Validación de tax codes, withholding codes y cálculos de retención.
- Política de redondeo: decimal, 2 posiciones, `MidpointRounding.AwayFromZero`, tolerancia `0.01`.
- Eventos Audit/Outbox `CreditNoteRulesValidated`, `DebitNoteRulesValidated`, `WithholdingRulesValidated` durante generación XML.
- XML validator reforzado para codDoc, clave de acceso, totales, motivos, documento relacionado, periodo fiscal e impuestos.

## Lo que NO representa

Este catálogo no es un catálogo oficial completo del SRI. Es una foundation interna controlada, versionable y auditable para desarrollo/test. Debe validarse funcional y normativamente antes de producción.

## Sin migración 012

No se creó `012_sri_catalogs_tax_rules.sql` porque el alcance quedó resuelto en dominio/configuración y no requiere persistencia nueva. `sri_catalog_items` existente queda disponible para una fase futura si se decide persistir catálogos por versión/tenant.

## Riesgos

- Catálogos foundation incompletos frente a normativa vigente.
- Retenciones requieren reglas específicas por impuesto, agente, régimen y porcentaje.
- XSD oficial y validación normativa completa siguen diferidas.
- SRI Test real sigue bloqueado por aprobación manual y custodia segura.

## Próximo paso

Sprint 3 P3 debería decidir entre persistir/versionar catálogos por tenant, integrar catálogos revisados por experto tributario o avanzar a XAdES/SRI Test controlado.
