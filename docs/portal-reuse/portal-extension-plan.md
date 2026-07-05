# Portal Extension Plan

## Objetivo

Definir cómo el módulo financiero extenderá el portal corporativo.

## Extensiones iniciales

### Security API

Agregar permisos:

```text
finance.access
accounting.chart.read
accounting.chart.manage
accounting.journal.create
accounting.journal.approve
accounting.journal.post
accounting.period.close
billing.invoice.create
billing.invoice.approve
billing.invoice.issue
billing.creditnote.create
billing.debitnote.create
tax.withholding.create
tax.catalog.manage
tax.ats.generate
sri.document.send
sri.document.reprocess
sri.document.cancel
sri.certificate.manage
```

### Menu API

Registrar menú:

```text
Financiero
├── Dashboard financiero
├── Contabilidad
│   ├── Plan de cuentas
│   ├── Asientos
│   ├── Periodos fiscales
│   └── Reportes contables
├── Facturación
│   ├── Facturas
│   ├── Notas de crédito
│   ├── Notas de débito
│   └── Clientes
├── Tributario Ecuador
│   ├── Retenciones
│   ├── Catálogos SRI
│   ├── ATS
│   └── Reportes tributarios
├── SRI
│   ├── Documentos electrónicos
│   ├── Reprocesos
│   ├── Certificados
│   └── Anulaciones
└── Integraciones
    ├── ERP
    ├── CRM
    └── Conectores
```

### Audit API

Agregar tipos de evento:

```text
InvoiceCreated
InvoiceApproved
InvoiceAuthorizedBySri
InvoiceRejectedBySri
CreditNoteAuthorizedBySri
DebitNoteAuthorizedBySri
WithholdingAuthorizedBySri
AccountingEntryPosted
FiscalPeriodClosed
AtsGenerated
ExternalErpSyncFailed
```

### Notification API

Agregar tipos de notificación:

```text
InvoiceAuthorizedBySri
InvoiceRejectedBySri
SriServiceUnavailable
CertificateExpiringSoon
CertificateExpired
TaxObligationDueSoon
AtsGenerated
AtsGenerationFailed
ApprovalPending
ExternalSyncFailed
```

### Configuration API

Agregar grupos:

```text
Finance
Accounting
Billing
TaxEcuador
SRI
ElectronicDocuments
ExternalIntegrations
```

### Content API

Usar para almacenar o referenciar:

```text
XML generado
XML firmado
XML autorizado
RIDE PDF
Respuesta recepción SRI
Respuesta autorización SRI
ATS generado
Evidencias de envío
```
