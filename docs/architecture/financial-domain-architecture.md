# Arquitectura de dominio Financiero

## Bounded contexts

| Contexto | Responsabilidad | Sprint |
|---|---|---|
| Accounting | Orquestación contable e invariantes comunes | 1 |
| ChartOfAccounts | Cuentas, jerarquía, naturaleza y vigencia | 1 |
| FiscalPeriods | Apertura, cierre y control temporal | 1 |
| JournalEntries | Cabecera, detalles, partida doble, contabilización y reverso | 1 |
| Billing | Facturación, notas, liquidaciones y guías | 2 |
| Tax | Impuestos, retenciones y reglas tributarias | 2 |
| ElectronicDocuments | XML y ciclo documental electrónico | 3; storage bloqueado |
| SriIntegration | Envío, recepción, autorización y resiliencia SRI | 3; productivo bloqueado |
| FiscalReporting | RIDE, ATS y reportes fiscales | 4; bloqueado |

## Sprint 1

Implementa únicamente ChartOfAccounts, FiscalPeriods y JournalEntries bajo Accounting. El asiento mantiene suma débito = crédito, moneda base definida, período abierto y transición explícita borrador → contabilizado → reversado. Las decisiones detalladas se aprobarán antes del código.

## Comunicación

- HTTP por Gateway para operaciones síncronas y capacidades Portal.
- Adaptadores para Audit y Notification.
- Eventos versionados con correlationId mediante Outbox local; Inbox para entradas idempotentes.
- Ningún contexto consulta tablas de otro contexto ni bases Portal.

## Persistencia

Cada bounded context posee esquema/base lógica y migraciones propias. No hay bases compartidas con PortalCorporativo. Compartir contratos técnicos no autoriza compartir entidades de dominio.

## Diferido

Billing, Tax, ElectronicDocuments, SriIntegration, FiscalReporting, firma/custodia, SRI productivo y frontend. Content/File, Reporting, Integration productiva, Angular Shell e IdP dependen de Portal Sprint 2.
