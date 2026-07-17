# Sprint 8 P4 - External Approval UX Evidence

## Scope

P4 hardens the External Approval Workflow UX for functional QA without activating productive tax flows. The screen remains foundation-only and depends on Portal-owned capabilities for identity, menu, configuration, content/file, notification, audit/outbox and runtime context.

## UX states covered

| State | Expected UX |
|---|---|
| Empty | Shows a safe empty state and explains that only foundation requests may be created with safe flags and manage permission. |
| Loading | Uses the shared loading state. |
| Error | Uses the shared sanitized error component. |
| Draft | Allows only foundation submit when commands are explicitly enabled. |
| Submitted | Guides the user to start review. |
| InReview | Allows only foundation decision flow when enabled. |
| ApprovedFoundation | Warns that approval does not enable production. |
| RejectedFoundation | Shows sanitized rejection semantics. |
| Blocked | Explains dependency/legal/tax/security blockers. |
| Superseded / Cancelled | Treats the request as non-productive. |

## Required disclaimers

- `ApprovedFoundation no habilita producción`.
- `Evidence reference is Portal-owned metadata only`.
- `Notification intent is prepared only; no send`.
- `External approval does not replace legal/tax approval`.
- `Production requires Portal + legal/tax/security approval`.

## Evidence reference checklist

- Provider is shown.
- Reference id is partially sanitized.
- Display name is sanitized.
- Content type and hash are metadata-only.
- Purpose is shown.
- UX states `Portal-owned` and `No file stored in Financiero`.
- No upload control is rendered.
- No download control is rendered.
- No payload, XML, base64, certificate or sensitive link is rendered.

## Notification intent checklist

- Intent is shown as foundation/no-send.
- Portal Notification remains owner.
- Delivery is pending a future Portal contract.
- No send/resend control is rendered.
- No email, Teams recipient or SMTP configuration is rendered.

## Blocked actions

- Productive approval remains blocked.
- SRI Test real and SRI production remain blocked.
- Official ATS, legal-final RIDE and productive XAdES remain blocked.
- Evidence upload/download remain blocked.
- Notification send remains blocked.
- Token storage and querystring token usage remain blocked.

## Verification commands

Run from repository root unless noted:

```powershell
git diff --check
dotnet restore Financiero.sln
dotnet build Financiero.sln --no-restore
$env:DOTNET_ROLL_FORWARD='Major'; dotnet test Financiero.sln --no-build
cd frontend/financiero-web
pnpm install --frozen-lockfile
pnpm run build
pnpm test
node tools/verify-frontend.mjs
node tools/verify-portal-e2e-contract.mjs
cd ../..
pwsh -NoProfile -ExecutionPolicy Bypass -File tools/validate-portal-financiero-e2e.ps1 -OutputMarkdown
docker compose config
```

The preflight may return exit code `2` when shared SQL or Portal Gateway are unavailable. That is acceptable as `BLOCKED_DEPENDENCY`, not an application failure.

## No production evidence

This P4 evidence uses textual checklist validation only. It does not include real taxpayer data, XML, SRI responses, certificates, personal data, secrets, screenshots with sensitive data or downloaded Portal files.

## Risks

- A full PASS still requires shared SQL and Portal Gateway to be running.
- Legal/tax/security approval remains external to this foundation UI.
- Portal Content/File and Notification productive contracts remain Portal-owned dependencies.

## Result

Observed local validation:

- `git diff --check`: passed with LF/CRLF warnings only.
- `dotnet restore Financiero.sln`: passed.
- `dotnet build Financiero.sln --no-restore`: passed.
- `DOTNET_ROLL_FORWARD=Major dotnet test Financiero.sln --no-build`: 322 passed.
- `pnpm install --frozen-lockfile`: already up to date.
- Angular build via bundled Node: passed.
- `node tools/verify-frontend.mjs`: passed.
- `node tools/verify-portal-e2e-contract.mjs`: passed.
- `docker compose config`: passed.
- `tools/validate-portal-financiero-e2e.ps1 -OutputMarkdown`: returned `SCRIPT_EXIT=2` with `BLOCKED_DEPENDENCY` because shared SQL `host.docker.internal:21433`, Financiero API `localhost:8083` and Portal Gateway `localhost:8082` were unavailable locally.

P4 is ready for PR validation. Full runtime PASS remains dependent on shared SQL and Portal Gateway availability.
