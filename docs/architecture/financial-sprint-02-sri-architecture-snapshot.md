# Financial Sprint 2 SRI Architecture Snapshot

## Resumen por capas

- API: endpoints de documentos electrónicos, readiness SRI, connectivity probe e integration status.
- Application: orquestación de factura, XML, firma, SRI mock/dry-run, storage metadata, RIDE y observabilidad.
- Domain: `ElectronicDocument`, líneas, impuestos, secuencias y estados.
- Infrastructure: EF/SQL común, health checks, adaptadores Portal development y configuración.

## Puertos y adaptadores

- Signature: `IElectronicSignatureService`, Development y XAdES foundation.
- CertificateProvider: `ICertificateProvider`, placeholders y `SecretStoreCertificateProvider`.
- SecretStore: `ISecretStoreClient`, Development/AzureKeyVault placeholder/External/Disabled.
- SRI Reception: `ISriReceptionClient`, Development y SOAP dry-run.
- SRI Authorization: `ISriAuthorizationClient`, Development y SOAP dry-run.
- Portal Content/File: `IElectronicDocumentStorageClient`, Development y contract readiness.
- RIDE/PDF: `IRidePdfGenerator`, Development placeholder.
- Observability: sanitizers, integration telemetry y readiness.

## Boundary Portal vs Financiero

Portal conserva ownership de Security, Gateway, Configuration, Audit, Notification, Content/File, logging/correlation y SQL Server local común. Financiero conserva ownership de reglas contables/SRI y solo define contratos/adaptadores consumidores.

## Flujo electrónico

1. Draft invoice.
2. Add line.
3. Generate XML.
4. Validate XML.
5. Sign dev/XAdES-ready.
6. Send mock/dry-run.
7. Authorize mock/dry-run.
8. Generate RIDE placeholder.
9. Store metadata/hash.

## Gates de seguridad

- `financial.sri.allowProduction=false`.
- `financial.sri.test.dryRun=true`.
- `financial.sri.test.allowDocumentSend=false`.
- `financial.sri.storage.sendPayloads=false`.
- `financial.secrets.provider=Disabled`.

## Estados y modos

Estados: Draft, Generated, Signed, Sent, Authorized, Rejected, Error. Mock/Development está funcional. Test dry-run está preparado. XAdES, SRI Test real y Content/File payload real son placeholders/readiness.
