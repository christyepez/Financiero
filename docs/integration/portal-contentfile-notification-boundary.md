# Portal Content/File and Notification Boundary

## What Financiero stores

- Provider name.
- Portal-owned reference id.
- Sanitized display name.
- Optional content type and hash.
- Optional purpose, size and retention hint for validation only.

## What Financiero does not store

- Files.
- XML payloads.
- SRI responses.
- Certificates or private keys.
- Base64 evidence.
- Tokenized links.
- Recipient lists or delivery provider credentials.

## API readiness

`GET /api/financial/external-approval-requests/integration-readiness`

Returns Content/File status, Notification status, Audit/Outbox status, feature flags, blockers and foundation disclaimer.

## Notification intents

Foundation intents are prepared for:

- `ExternalApprovalRequestSubmitted`
- `ExternalApprovalReviewStarted`
- `ExternalApprovalDecisionRecorded`
- `ExternalApprovalRequestCancelled`

They are not sent and contain no sensitive payload.
