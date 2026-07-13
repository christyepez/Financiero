# XAdES Signature Strategy

## Puerto de firma

`IElectronicSignatureService` recibe XML sin firmar y `SignatureContext`. Devuelve `SignatureResult` con XML firmado, provider, alias de certificado, timestamp, digest y modo de firma.

## Providers

- Development: simula firma; permitido solo fuera de Production.
- Disabled: bloquea firma.
- External: reservado para proveedor externo seguro.
- LocalCertificatePlaceholder: falla explícitamente hasta implementar lectura segura.

## Qué NO se implementa aún

- Firma XAdES-BES/EPES productiva.
- Lectura de `.p12/.pfx`.
- Passwords de certificados.
- Custodia real de claves privadas.

## Secret store / Key Vault

La integración productiva debe usar `financial.sri.signature.certificateSecretName`, `keyVaultName` y password referenciado por secreto. El repo y la base de datos nunca deben contener certificados ni passwords.

## Rotación

La rotación debe hacerse cambiando alias/versiones en secret store, auditando cada uso y manteniendo separación Test/Production.

## Metadata guardada

- `SignatureProvider`.
- `SignatureDigest`.
- `SignedXmlContentHash`.
- `SignedXmlStorageId`.

## Checklist para habilitar firma real

## Sprint 2 P3

Se agrega `XadesElectronicSignatureService` y providers placeholder:

- `DevelopmentCertificateProvider`.
- `KeyVaultCertificateProviderPlaceholder`.
- `LocalCertificateProviderPlaceholder`.

`LocalCertificateProviderPlaceholder` rechaza Production. `KeyVault` no lee secretos todavía; solo valida nombres de configuración y falla claro. No deben versionarse `.p12`, `.pfx`, `.key`, `.cer`, `.crt` ni `.pem`.

## Sprint 2 P4

`SecretStoreCertificateProvider` usa `SecretReference` contra `ISecretStoreClient`. P4 no firma con certificado real: si el secreto se resuelve, el provider conserva el contrato y falla con `sri.certificate.xades.not_enabled` hasta implementar firma XAdES real.

## Sprint 2 P5

La ruta esperada queda documentada como Secret Store -> Certificate Provider -> XAdES Service. No se cargan archivos locales ni certificados reales. `certificatePasswordSecretName` es una referencia, no un valor plano.

- Certificado válido en secret store.
- Password en secret store.
- Adapter XAdES probado.
- Pruebas con ambiente SRI Test.
- Auditoría de uso de certificado.
- Política de rotación aprobada.
