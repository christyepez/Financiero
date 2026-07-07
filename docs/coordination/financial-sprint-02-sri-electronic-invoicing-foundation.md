# Sprint 2 P1 — SRI & Electronic Invoicing Foundation

## Alcance

P1 introduce la base funcional y técnica para documentos electrónicos Ecuador/SRI sin envío productivo. Implementa factura electrónica foundation, clave de acceso, XML base, puertos de firma, clientes SRI dev/mock, permisos runtime, migraciones y smoke local.

## Implementado

- Dominio `ElectronicDocument`, líneas, impuestos, estados, tipos, ambientes y emisión.
- Tipos modelados: factura, nota de crédito, nota de débito, retención y guía de remisión.
- Flujo funcional completo solo para factura base.
- Generación de clave de acceso SRI con módulo 11.
- Secuencias tributarias independientes de asientos contables.
- XML base de factura con `infoTributaria`, `infoFactura` y `detalles`.
- `DevelopmentElectronicSignatureService` sin firma real.
- `DevelopmentSriReceptionClient` y `DevelopmentSriAuthorizationClient` sin SRI real.
- Audit/Outbox mediante puertos existentes.
- Migraciones `007` y `008`.
- Smoke mock `scripts/smoke/financial-sri-smoke.ps1`.

## No implementado

- Firma XAdES productiva.
- Envío SOAP real a SRI.
- Certificados reales o secretos.
- PDF/RIDE.
- Frontend Angular.
- Retenciones, notas y guías funcionalmente completas.
- Almacenamiento XML/PDF propio.
- Contabilización automática final con Journal Entries.

## Decisión

Financiero prepara puertos y flujo mock; producción requiere cerrar custodia de certificados, endpoints SRI, validación normativa vigente y almacenamiento documental con Portal Content/File.
