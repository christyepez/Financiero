# Sprint 7 Security Checklist

## Secrets and tokens

Control tokens: No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

- No tokens, passwords, private URLs or real connection strings in repository.
- Frontend must not use `localStorage`, `sessionStorage` or cookies for auth.
- Delegated auth remains in-memory and Portal-owned.

## Certificates

- No `.p12`, `.pfx`, `.pem`, `.key`, `.cer`, `.crt` files.
- No productive XAdES.
- Certificate custody requires external vault, rotation and manual approval.

## XML, evidence and PII

- No real taxpayer XML.
- No real SRI responses.
- No real evidence files.
- Evidence references are metadata-only and sanitized.
- Logs must redact access keys, IDs, XML, tokens and payloads.

## Portal ownership

- Security/Auth: Portal.
- Menu: Portal.
- Configuration: Portal.
- Audit/Outbox: Portal-owned contract.
- Content/File: Portal.
- Notification: Portal.

## Dangerous flags

Must remain false by default:

- `allowProductiveActivation`
- `allowOfficialTaxFlows`
- `allowSriSubmission`
- `allowAtsOfficialActions`
- `allowEvidenceUpload`
- `allowNotificationSend`
- `allowXmlPreviewUi`

## Before any production activation

- Legal/tax approval.
- Security approval.
- Certificate custody approval.
- Portal Content/File real contract.
- Portal Notification real contract.
- SRI Test evidence with synthetic data.
- Rollback plan.
- Audit evidence.
- QA sign-off.
