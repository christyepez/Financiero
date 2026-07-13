# Secure Secret Store Strategy

Financiero no almacena secretos ni certificados en repositorio, base de datos, logs o respuestas API.

## Puerto

`ISecretStoreClient` resuelve `SecretReference` y devuelve `SecretStoreResult`.

Providers:

- `DevelopmentSecretStoreClient`: solo Development y con `financial.secrets.allowDevelopmentSecrets=true`.
- `AzureKeyVaultSecretStoreClientPlaceholder`: valida `financial.secrets.keyVaultName` y falla claro hasta integrar SDK/credenciales.
- `ExternalSecretStoreClientPlaceholder`: contrato para proveedor externo.
- `DisabledSecretStoreClient`: default seguro.

## Configuración

- `financial.secrets.provider`.
- `financial.secrets.keyVaultName`.
- `financial.secrets.allowDevelopmentSecrets`.
- `financial.secrets.maskSecretsInLogs`.

## Reglas

- No versionar `.p12`, `.pfx`, `.key`, `.cer`, `.crt`, `.pem`.
- No guardar bytes de certificados en DB.
- No loguear valores de secretos.
- Usar referencias y alias enmascarados.
