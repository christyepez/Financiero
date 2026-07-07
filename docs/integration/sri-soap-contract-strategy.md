# SRI SOAP Contract Strategy

## Recepción

`ISriReceptionClient.SendAsync` recibe `SriReceptionRequest` con access key, XML firmado y contexto. Devuelve `SriReceptionResponse`.

## Autorización

`ISriAuthorizationClient.AuthorizeAsync` recibe `SriAuthorizationRequest` con access key y contexto. Devuelve `SriAuthorizationResponse`.

## Estados normalizados

- Received.
- Returned.
- Processing.
- Authorized.
- Rejected.
- NotFound.
- Error.

## Ambientes

Test y Production se configuran por `financial.sri.environment` y URLs externas. P2 no llama producción.

## Timeouts y retries

Config:

- `financial.sri.timeoutSeconds`.
- `financial.sri.maxRetries`.
- `financial.sri.retryDelaySeconds`.

## Política

Si `financial.sri.integration.mode` no es Mock/Development y no existe cliente real configurado, el sistema falla con error claro. No se hardcodean URLs productivas.

## Checklist real

- Confirmar URLs oficiales vigentes.
- Validar certificados TLS.
- Probar con ambiente Test.
- Mapear errores SRI completos.
- Definir política de reintentos e idempotencia.
