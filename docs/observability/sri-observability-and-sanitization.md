# SRI Observability and Sanitization

## Campos permitidos

- correlationId
- tenantId
- documentId
- documentType
- status
- provider
- mode
- durationMs
- attemptNumber
- accessKeyMasked
- hash
- errorCode
- sanitizedMessage

## Campos prohibidos

- XML completo
- certificados
- passwords
- secret values
- identificación completa
- clave de acceso completa
- respuesta SRI completa con datos sensibles

## Helpers

- `SriSensitiveDataSanitizer`
- `SecretMaskingHelper`
- `SriIntegrationLogSanitizer`
- `IntegrationAttemptTelemetry`
- `SriObservabilityEvent`

Los eventos funcionales siguen saliendo por Audit/Outbox existentes; no se crea motor paralelo.
