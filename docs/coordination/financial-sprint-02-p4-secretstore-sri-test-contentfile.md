# Sprint 2 P4 - Secure Secret Store, SRI Test y Portal Content/File Readiness

P4 prepara integración productiva controlada sin activar producción ni versionar material sensible.

## Entregado

- Puerto `ISecretStoreClient` y providers Development, Azure Key Vault placeholder, External placeholder y Disabled.
- `SecretStoreCertificateProvider` para resolver certificados vía referencia segura.
- SRI SOAP Test dry-run con validación de URLs y bloqueo de Production.
- Sanitización de clave de acceso, identificación, XML y mensajes.
- Readiness SRI por `/health/sri` y `/api/financial/electronic-documents/sri/readiness`.
- Contract readiness de Portal Content/File con metadata/hash y payloads desactivados por defecto.

## Gates

- `financial.sri.allowProduction=false` por defecto.
- `financial.sri.test.dryRun=true` por defecto.
- `financial.sri.logPayloads=false`.
- `financial.sri.maskPayloads=true`.
- `financial.secrets.provider=Disabled` por defecto.

## No incluido

- Firma XAdES real con certificado.
- SRI producción.
- Persistencia documental propia.
- Payload real a Portal Content/File.
