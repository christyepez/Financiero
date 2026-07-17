# Sprint 7 P3 - Portal Content/File and Notification Boundary

## Scope

Foundation only. Financiero prepares safe metadata contracts for external approval evidence references and notification intents.

## Content/File boundary

- PortalCorporativo owns Content/File.
- Financiero stores reference metadata only.
- No upload, download, binary payload, XML payload, certificate, private key or tokenized URL is accepted.
- Evidence is displayed as `reference only / Portal-owned evidence`.

## Notification boundary

- PortalCorporativo owns Notification.
- Financiero prepares notification intents through Outbox foundation events.
- No SMTP, Teams connector, email delivery or recipient resolution is implemented in Financiero.

## Feature flags

- `allowPortalContentFileEvidenceReferences=false` by default.
- `allowPortalNotificationIntents=false` by default.
- `allowNotificationSend=false` always.
- `allowEvidenceUpload=false` always.

## Validation

P3 adds backend validators, API readiness and frontend readiness display without adding storage columns or production behavior.
