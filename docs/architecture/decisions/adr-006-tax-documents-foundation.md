# ADR-006 - Tax Documents Foundation

## Estado

Aprobado para Sprint 3 P1.

## Contexto

Sprint 2 dejó preparada la base de facturación electrónica: `ElectronicDocument`, secuencias SRI, clave de acceso, XML foundation, firma development/mock, SRI mock/test dry-run, storage delegado a Portal Content/File y gates de seguridad.

Financiero necesita soportar nota de crédito, nota de débito y retención sin duplicar plataforma ni adelantar producción SRI.

## Decisión

Se extiende el agregado `ElectronicDocument` para representar documentos tributarios adicionales:

- `CreditNote` con `codDoc=04`.
- `DebitNote` con `codDoc=05`.
- `Withholding` con `codDoc=07`.

Se agregan entidades hijas para referencias, motivos de débito e impuestos retenidos. Se mantiene la misma infraestructura transversal:

- Security Portal mediante permisos `financial.electronicdocuments.*`.
- Audit/Outbox mediante puertos existentes.
- Storage XML/PDF delegado a Portal Content/File mediante placeholder.
- SQL común con base lógica `FinancieroDb`.

## Consecuencias

- Los consumidores financieros pueden crear y validar documentos tributarios foundation sin esperar frontend.
- La secuencia y la clave de acceso separan el tipo documental por `codDoc`.
- La persistencia agrega tablas específicas pero no crea otro bounded context ni otro SQL Server.
- La validación XML sigue siendo básica; XSD oficial y reglas tributarias completas quedan diferidas.

## No decidido

- Catálogos SRI productivos completos.
- Firma XAdES real con certificado.
- Envío real a SRI Test/Production.
- RIDE final por tipo documental.
- Integración contable automática de NC/ND/retenciones.

## Controles

- Provider development/mock continúa como default local.
- Producción SRI permanece bloqueada por configuración.
- No se versionan certificados, secretos ni XML reales.
- Los endpoints nuevos exigen permisos runtime.
