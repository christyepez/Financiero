# External Review Gate

All gates start as `Open`. None of these gates approve production by themselves; they define evidence required before future implementation.

| Gate | Owner suggested | Required evidence | Initial status | Approval criteria | Blocking criteria | Risk if skipped |
|---|---|---|---|---|---|---|
| Tax expert review | Ecuador tax expert | Synthetic samples, rules mapping, tax totals and catalog evidence | Open | Written approval for scope | Any critical tax gap open | Incorrect tax output |
| Legal RIDE layout review | Legal reviewer | Final layout samples, mandatory texts, QR/access-key decision | Open | Layout approved for each document type | Missing required text or authorization evidence | Non-compliant RIDE |
| SRI schema/catalog review | Tax/SRI reviewer | Official schema/catalog versions and mapping | Open | Versioned approval and validation evidence | Missing schema/catalog | Invalid ATS/SRI payload |
| Security production gate | Security reviewer | Permissions, redaction, logs, rollback, incident plan | Open | No real secret/XML exposure paths | Secrets/certs/logging risks open | Data leakage |
| Certificate custody approval | Security/operations | Custody model, vault reference, rotation procedure | Open | Non-repo custody and rotation approved | Local/real cert in repo or unclear custody | Certificate compromise |
| Portal Content/File contract approval | Portal owner | Contract version, auth, retention, dry-run evidence | Open | Stable contract and ownership accepted | Payload/storage ownership unclear | Duplicated or unsafe storage |
| SRI Test validation evidence | Integration owner | Sanitized test evidence with synthetic data | Open | Accepted test evidence and error mapping | Real data or no evidence | Unknown integration failure |
| Purchases/voided ATS foundation review | Tax/SRI reviewer | Synthetic purchase and voided records mapped to ATS sections | Open | Approval of support/document type mapping | Incomplete purchase/voided mapping | Invalid ATS purchases/anulados section |
| Operational runbook approval | Operations | Runbook, support model, rollback and monitoring | Open | Runbook approved by operations | No support/rollback path | Operational outage |
| Production deployment approval | Business/release owner | All previous gates closed | Open | Explicit production go/no-go | Any prior gate open | Premature production activation |
# Sprint 5 external approval gates

External approval readiness is exposed as foundation/read-only metadata. It does not enable production.

Required gates:

- ATS official XML.
- RIDE legal final.
- XAdES signature and certificate custody.
- SRI Test submission.
- SRI Production submission.
- Portal Content/File production payload contract.
- Security production gate.
- Operational runbook.
