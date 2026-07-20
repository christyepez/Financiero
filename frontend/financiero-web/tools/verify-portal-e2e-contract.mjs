import { readFileSync, statSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { join } from 'node:path';

const root = fileURLToPath(new URL('..', import.meta.url));
const repoRoot = fileURLToPath(new URL('../../..', import.meta.url));

const files = {
  models: 'src/app/core/portal-shell/portal-shell.models.ts',
  validation: 'src/app/core/portal-shell/portal-shell-validation.ts',
  defaults: 'src/app/core/portal-shell/portal-shell.defaults.ts',
  context: 'src/app/core/portal-shell/portal-context.adapter.ts',
  auth: 'src/app/core/adapters/portal-auth.adapter.ts',
  menu: 'src/app/core/adapters/portal-menu.adapter.ts',
  guard: 'src/app/core/services/command-guard.service.ts',
  api: 'src/app/core/services/portal-integration-readiness-api.service.ts',
  dashboard: 'src/app/features/dashboard/dashboard.component.ts',
  app: 'src/app/app.component.ts'
};

for (const file of Object.values(files)) statSync(join(root, file));

const read = file => readFileSync(join(root, file), 'utf8');
const models = read(files.models);
const validation = read(files.validation);
const defaults = read(files.defaults);
const context = read(files.context);
const auth = read(files.auth);
const menu = read(files.menu);
const guard = read(files.guard);
const api = read(files.api);
const dashboard = read(files.dashboard);
const app = read(files.app);
const externalApproval = readFileSync(join(root, 'src/app/features/external-approvals/external-approvals.component.ts'), 'utf8');

for (const token of ['SUPPORTED_PORTAL_SHELL_CONTRACT_VERSION', 'PortalShellSource', "'standalone' | 'portal'", 'issuedAt', 'expiresAt', 'permissions', 'menu', 'featureFlags', 'correlationId']) {
  if (!models.includes(token)) throw new Error(`PortalShellContext contract missing ${token}.`);
}
for (const token of ['sanitizeContractVersion', 'sanitizeMenuItems', 'sanitizeDelegatedToken', 'Portal integrated context missing required user']) {
  if (!validation.includes(token)) throw new Error(`PortalShellContext validation missing ${token}.`);
}
for (const flag of ['allowProductizationReadiness: true', 'allowPortalContentFileEvidenceReferences: false', 'allowPortalNotificationIntents: false', 'allowExternalApprovalCommands: false', 'allowPurchaseCommands: false', 'allowVoidedDocumentCommands: false', 'allowProductiveActivation: false', 'allowOfficialTaxFlows: false', 'allowSriSubmission: false', 'allowAtsOfficialActions: false', 'allowNotificationSend: false', 'allowEvidenceUpload: false']) {
  if (!defaults.includes(flag)) throw new Error(`Safe feature flag default missing ${flag}.`);
}
for (const token of ["context.environment.shellMode !== 'portal-integrated'", 'context.expiresAt', 'environment.production || context.environment.shellMode ===']) {
  if (!auth.includes(token)) throw new Error(`Delegated auth safety missing ${token}.`);
}
for (const token of ['allowedFinancialRoutes', '/dashboard', '/purchases', '/voided-documents']) {
  if (!defaults.includes(token)) throw new Error(`Menu allow-list definition missing ${token}.`);
}
for (const token of ['hasPermission', "!item.route.startsWith('//')"]) {
  if (!menu.includes(token)) throw new Error(`Menu allow-list validation missing ${token}.`);
}
for (const token of ['canActivateProduction', 'allowProductiveActivation', 'allowOfficialTaxFlows']) {
  if (!guard.includes(token)) throw new Error(`Command guard production block missing ${token}.`);
}
for (const token of ['/api/financial/portal-integration/readiness', 'Portal E2E readiness', 'development standalone only', 'production requires Portal context', 'SQL común requerido', 'BLOCKED DEPENDENCY']) {
  const combined = `${api}\n${dashboard}`;
  if (!combined.includes(token)) throw new Error(`Portal E2E UI/API missing ${token}.`);
}
for (const token of ['ApprovedFoundation no habilita producción', 'Evidence reference is Portal-owned metadata only', 'Notification intent is prepared only; no send', 'Portal Notification owner', 'No file stored in Financiero']) {
  if (!externalApproval.includes(token)) throw new Error(`External approval Portal boundary missing ${token}.`);
}
for (const token of ['missingRequiredPortalContext()', 'hasUnsupportedContract()', 'fin-portal-context-required']) {
  if (!app.includes(token)) throw new Error(`Production standalone block missing ${token}.`);
}
for (const doc of [
  'docs/integration/portal-e2e-validation-checklist.md',
  'docs/runbooks/portal-financiero-local-e2e-runbook.md',
  'docs/qa/financial-sprint-08-p1-e2e-evidence-template.md',
  'docs/qa/financial-sprint-08-p2-e2e-execution-evidence.md',
  'docs/runbooks/shared-sql-runtime-validation.md',
  'docs/runbooks/financial-qa-env-template.md',
  'docs/runbooks/financial-health-troubleshooting.md',
  'docs/qa/financial-sprint-08-p3-qa-infra-stabilization-evidence.md',
  'docs/qa/financial-sprint-08-p4-external-approval-ux-evidence.md',
  'docs/coordination/financial-sprint-08-closure.md',
  'docs/coordination/financial-sprint-09-closure.md',
  'docs/coordination/financial-sprint-10-p1-owner-evidence-intake.md',
  'docs/qa/financial-sprint-08-final-e2e-evidence.md',
  'docs/qa/financial-sprint-09-p1-real-e2e-infra-evidence.md',
  'docs/qa/financial-sprint-09-p2-dependency-diagnostic-evidence.md',
  'docs/qa/financial-sprint-09-p3-pass-or-blocked-evidence.md',
  'docs/qa/financial-sprint-09-p4-infra-intervention-evidence.md',
  'docs/qa/financial-sprint-09-final-infra-evidence.md',
  'docs/qa/financial-sprint-10-p1-e2e-acceptance-gate.md',
  'docs/qa/templates/sql-common-evidence-template.md',
  'docs/qa/templates/portal-gateway-evidence-template.md',
  'docs/qa/templates/portal-shell-evidence-template.md',
  'docs/qa/templates/portal-contract-evidence-template.md',
  'docs/runbooks/start-shared-sql-and-portal-runtime.md',
  'docs/runbooks/financial-e2e-pass-checklist.md',
  'docs/runbooks/financial-e2e-dependency-owner-handoff.md',
  'docs/runbooks/infra-sql-common-intervention-package.md',
  'docs/runbooks/portal-runtime-intervention-package.md',
  'docs/roadmap/financial-external-dependency-backlog.md',
  'docs/roadmap/financial-sprint-10-decision-matrix.md',
  'docs/roadmap/financial-sprint-09-decision-matrix.md',
  'docs/roadmap/financial-controlled-productization-backlog.md',
  'docs/architecture/financial-risk-register.md',
  'docs/releases/financial-sprint-08-release-notes.md',
  'tools/validate-portal-financiero-e2e.ps1'
]) statSync(join(repoRoot, doc));

for (const doc of [
  'docs/qa/financial-sprint-08-final-e2e-evidence.md',
  'docs/qa/financial-sprint-09-p1-real-e2e-infra-evidence.md',
  'docs/qa/financial-sprint-09-p2-dependency-diagnostic-evidence.md',
  'docs/qa/financial-sprint-09-p3-pass-or-blocked-evidence.md',
  'docs/qa/financial-sprint-09-p4-infra-intervention-evidence.md',
  'docs/qa/financial-sprint-09-final-infra-evidence.md',
  'docs/runbooks/start-shared-sql-and-portal-runtime.md',
  'docs/runbooks/financial-e2e-pass-checklist.md',
  'docs/runbooks/financial-e2e-dependency-owner-handoff.md',
  'docs/runbooks/infra-sql-common-intervention-package.md',
  'docs/runbooks/portal-runtime-intervention-package.md',
  'docs/roadmap/financial-external-dependency-backlog.md',
  'docs/roadmap/financial-sprint-10-decision-matrix.md',
  'docs/roadmap/financial-sprint-09-decision-matrix.md',
  'docs/roadmap/financial-controlled-productization-backlog.md'
]) {
  const text = readFileSync(join(repoRoot, doc), 'utf8');
  for (const token of ['BLOCKED_DEPENDENCY', 'Portal Gateway', 'shared SQL', 'not production-ready']) {
    if (!text.includes(token)) throw new Error(`${doc} missing Portal E2E closure token ${token}.`);
  }
}

for (const doc of [
  'docs/coordination/financial-sprint-09-closure.md',
  'docs/coordination/financial-sprint-10-p1-owner-evidence-intake.md',
  'docs/qa/financial-sprint-09-final-infra-evidence.md',
  'docs/qa/financial-sprint-10-p1-e2e-acceptance-gate.md',
  'docs/qa/templates/sql-common-evidence-template.md',
  'docs/qa/templates/portal-gateway-evidence-template.md',
  'docs/qa/templates/portal-shell-evidence-template.md',
  'docs/qa/templates/portal-contract-evidence-template.md',
  'docs/roadmap/financial-sprint-10-decision-matrix.md',
  'docs/roadmap/financial-external-dependency-backlog.md',
  'docs/releases/financial-sprint-09-release-notes.md'
]) {
  statSync(join(repoRoot, doc));
  const text = readFileSync(join(repoRoot, doc), 'utf8');
  for (const token of ['BLOCKED_DEPENDENCY', 'Portal Gateway', 'shared SQL', 'not production-ready']) {
    if (!text.includes(token)) throw new Error(`${doc} missing Sprint 9 P5 Portal closure token ${token}.`);
  }
}

const forbiddenQueryTokens = ['access_token=', 'id_token=', 'refresh_token=', 'token='];
for (const file of [models, validation, context, auth, api, dashboard]) {
  for (const token of forbiddenQueryTokens) {
    if (file.includes(token)) throw new Error(`Querystring token pattern must not appear: ${token}.`);
  }
}

const preflight = readFileSync(join(repoRoot, 'tools/validate-portal-financiero-e2e.ps1'), 'utf8');
for (const token of ['PortalGatewayHealthPath', 'PortalShellHealthPath', 'FinancialApiHealthPath', 'EvidenceOutputPath', 'OwnerEvidenceRequired', 'AcceptanceGateSummary', 'HEALTH_PATH_NOT_CONFIRMED', 'HEALTH_ROUTE_ALTERNATIVE_REQUIRED', 'HTTP_STATUS_UNEXPECTED']) {
  if (!preflight.includes(token)) throw new Error(`Sprint 9 P4 preflight health route configuration missing ${token}.`);
}

console.log('Portal E2E contract checks passed.');
