# Sprint 7 Release Notes

## Status

Sprint 7 is closed as Portal integration readiness. It is not production-ready for tax filing or SRI operations.

Control tokens: No SRI Production; No official ATS; No legal-final RIDE; No productive XAdES.

## PRs

- P1: Real Portal Shell Contract Integration Hardening.
- P2: External Approval Workflow Persistence Foundation.
- P3: Portal Content/File and Notification Integration Boundary.
- P4: Controlled Productization Readiness for Purchases and Voided Documents.
- P5: Sprint Closure, QA Evidence and Sprint 8 Readiness.

## Base

- P4 merge/base validated on `main`: `4712f3894213b5938467bc2a32cea6f95e399ecb`.

## Delivered capabilities

- Portal Shell contract hardening.
- External approval request persistence foundation.
- Content/File and Notification readiness boundaries.
- Purchase and voided document productization readiness.
- Frontend flags and disclaimers for blocked production.
- QA/security documentation and Sprint 8 roadmap.

## Validations

- Backend restore/build/test expected.
- Frontend install/build/test expected.
- Docker compose config expected.
- Runtime health requires shared SQL Server.

## Risks and blockers

- Real Portal integration must be validated end-to-end.
- Shared SQL must be available for repeatable health.
- Tax/legal review remains mandatory.
- Production SRI, official ATS, legal RIDE and productive XAdES remain blocked.

## Recommendation

Start Sprint 8 with real PortalCorporativo E2E validation and stable shared SQL QA before any productization path.
