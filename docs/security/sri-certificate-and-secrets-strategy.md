# SRI Certificate and Secrets Strategy

## Regla P1

No se suben certificados reales, `.p12`, `.pfx`, `.key`, `.cer`, `.crt`, `.pem`, secretos ni passwords al repositorio.

## Configuración prevista

- `financial.sri.signature.provider`
- `financial.sri.signature.certificateSecretName`
- `financial.sri.signature.keyVaultName`

## Desarrollo

`DevelopmentElectronicSignatureService` no firma realmente. Devuelve XML marcado con firma simulada para validar flujo de estados y smoke.

## Producción futura

Debe usarse un secret store o Key Vault. La API solo recibirá referencias lógicas al certificado y nunca persistirá el material criptográfico en base de datos.

## Riesgos

- Custodia y rotación de certificados.
- Password del certificado.
- Auditoría de uso de firma.
- Separación de ambientes Test/Production SRI.
