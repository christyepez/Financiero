# ADR-025 - Portal Content/File and Notification Boundary

## Status

Accepted.

## Decision

Financiero will not own document storage or notification delivery. External approval evidence remains a sanitized reference to Portal Content/File. Notification actions remain foundation intents emitted through Outbox metadata only.

## Consequences

- No evidence files are stored by Financiero.
- No notification is sent by Financiero.
- PortalCorporativo must provide production Content/File and Notification contracts before any real evidence delivery or notification delivery is enabled.
- Approval decisions remain foundation and do not enable SRI production, ATS official output, RIDE legal final output or XAdES production.

## Security rules

Reject base64, XML, certificates, private keys, local paths, tokenized URLs and embedded/binary payloads.
