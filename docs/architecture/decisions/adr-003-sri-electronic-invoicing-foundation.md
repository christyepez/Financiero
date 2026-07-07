# ADR-003 — SRI Electronic Invoicing Foundation

## Estado

Aprobado para Sprint 2 P1.

## Contexto

Financiero necesita preparar documentos electrónicos Ecuador/SRI sin comprometer seguridad, certificados ni envío productivo prematuro.

## Decisión

Implementar foundation con dominio propio financiero, clave de acceso SRI, XML base de factura, puertos de firma y puertos de clientes SRI. Usar adapters Development/Mock en P1. No almacenar certificados ni secretos en repo/base de datos.

## Consecuencias

- Se habilita diseño incremental de facturación electrónica.
- Producción queda bloqueada hasta cerrar certificados, firma XAdES, endpoints SRI, validación normativa y almacenamiento documental.
- Portal sigue siendo owner de Security, Audit, Outbox, Configuration, Gateway y capacidades transversales.
