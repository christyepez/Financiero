# Sprint 3 P1 - Tax Documents Foundation

## Objetivo

Agregar foundation funcional para documentos tributarios SRI adicionales sin activar producción:

- Nota de Crédito, `codDoc=04`.
- Nota de Débito, `codDoc=05`.
- Comprobante de Retención, `codDoc=07`.

El paquete reutiliza `ElectronicDocument`, permisos `financial.electronicdocuments.*`, Audit/Outbox, Gateway, health, logging/correlationId y el SQL Server común. No implementa firma XAdES productiva, envío real SRI, RIDE final, frontend Angular ni almacenamiento documental propio.

## Alcance implementado

- Nuevos drafts para nota de crédito, nota de débito y retención.
- Referencia obligatoria al documento sustento/modificado.
- Motivos de nota de débito.
- Impuestos retenidos de comprobante de retención.
- Generadores XML foundation con raíces `notaCredito`, `notaDebito` y `comprobanteRetencion`.
- Validadores XML básicos por tipo documental.
- Endpoints mínimos protegidos por permisos existentes.
- Migración `011_tax_documents_foundation.sql`.
- Smoke local extendido para factura + NC + ND + retención en modo development/mock.

## Exclusiones

- No se envía a SRI producción.
- No se usan certificados reales.
- No se almacenan XML reales ni PII real en el repositorio.
- No se duplica Content/File del Portal.
- No se crean capacidades propias de Security, Audit, Notification, Gateway ni Outbox.
- No se implementan liquidaciones, guías de remisión, ATS ni reporting fiscal.

## Criterios de aceptación

- `codDoc` 04, 05 y 07 se generan en la clave de acceso.
- XML inválido no pasa validación básica.
- Los endpoints nuevos requieren permisos runtime.
- La migración es idempotente.
- La solución compila y los tests pasan.
- Docker mantiene el SQL Server común y no define SQL Server propio.

## Riesgos restantes

- La estructura XML es foundation y requiere contraste funcional final contra XSD/normativa SRI antes de producción.
- Retenciones completas requieren catálogos SRI ampliados y reglas tributarias por impuesto.
- RIDE por tipo documental queda para un sprint posterior.
- Firma XAdES real y SRI Test manual siguen bloqueados por custodia segura y aprobación.

## Próximo paso

Sprint 3 P2 debe decidir entre:

1. endurecer catálogos/reglas tributarias de NC/ND/retenciones;
2. activar XAdES real controlado si ya existe custodia segura;
3. implementar adapter productivo Portal Content/File cuando el contrato del Portal esté estable.
