import { readFileSync, readdirSync, statSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { basename, join } from 'node:path';

const root = fileURLToPath(new URL('..', import.meta.url));
const repoRoot = fileURLToPath(new URL('../../..', import.meta.url));
const required = [
  'src/app/core/adapters/portal-auth.adapter.ts',
  'src/app/core/adapters/portal-menu.adapter.ts',
  'src/app/core/interceptors/api-authorization.interceptor.ts',
  'src/app/core/interceptors/error-sanitization.interceptor.ts',
  'src/app/core/services/sanitization.service.ts',
  'src/app/core/portal-shell/portal-shell.models.ts',
  'src/app/core/portal-shell/portal-context.adapter.ts',
  'src/app/core/adapters/portal-configuration.adapter.ts',
  'src/app/core/adapters/portal-feature-flag.adapter.ts',
  'src/app/core/adapters/portal-telemetry.adapter.ts',
  'src/app/core/services/command-guard.service.ts',
  'src/app/core/services/portal-integration-readiness-api.service.ts',
  'src/app/shared/components/loading-state.component.ts',
  'src/app/shared/components/period-selector.component.ts',
  'src/app/shared/components/shell-mode-banner.component.ts',
  'src/app/shared/components/portal-context-required.component.ts',
  'src/app/shared/components/portal-context-error.component.ts',
  'src/app/shared/components/shell-contract-version-badge.component.ts',
  'src/app/shared/components/command-disabled-banner.component.ts',
  'src/app/shared/components/foundation-command-disclaimer.component.ts',
  'src/app/features/purchases/purchase-create-form.component.ts',
  'src/app/features/voided-documents/voided-document-create-form.component.ts',
  'src/app/features/sri-readiness/sri-readiness.component.ts',
  'src/app/features/ats-readiness/ats-readiness.component.ts',
  'src/app/features/external-approvals/external-approvals.component.ts',
  'src/environments/environment.ts',
  'src/environments/environment.development.ts',
  'src/environments/environment.example.ts'
];

for (const file of required) {
  statSync(join(root, file));
}

const forbidden = [
  'localStorage',
  'sessionStorage',
  'document.cookie',
  'console.log',
  'financial-login',
  'BEGIN PRIVATE KEY',
  'BEGIN CERTIFICATE',
  '.p12',
  '.pfx',
  '.pem',
  '.key',
  'sri.gob.ec',
  '<factura'
];

function walk(directory) {
  return readdirSync(directory).flatMap(name => {
    const full = join(directory, name);
    const stat = statSync(full);
    if (stat.isDirectory() && ['node_modules', 'dist', '.angular', 'coverage'].includes(basename(full))) {
      return [];
    }
    return stat.isDirectory() ? walk(full) : [full];
  });
}

for (const file of walk(join(root, 'src'))) {
  const text = readFileSync(file, 'utf8');
  for (const pattern of forbidden) {
    if (text.includes(pattern)) {
      throw new Error(`Forbidden frontend pattern "${pattern}" found in ${file}`);
    }
  }
}

for (const file of walk(root)) {
  if (file.endsWith('.env') || file.endsWith('.env.local')) {
    throw new Error(`Environment secret file must not be committed: ${file}`);
  }
  if (/\.(p12|pfx|pem|key|cer|crt)$/i.test(file)) {
    throw new Error(`Certificate or key file must not be committed: ${file}`);
  }
}

const apiService = readFileSync(join(root, 'src/app/core/services/api.service.ts'), 'utf8');
if (!apiService.includes('ApiEnvelope') || !apiService.includes('unwrap')) {
  throw new Error('ApiService must unwrap backend ApiResponse envelopes.');
}

const authAdapter = readFileSync(join(root, 'src/app/core/adapters/portal-auth.adapter.ts'), 'utf8');
if (!authAdapter.includes("environment.production || context.environment.shellMode === 'portal-integrated'")) {
  throw new Error('Development permissions must be disabled in production.');
}
if (!authAdapter.includes("context.environment.shellMode !== 'portal-integrated'") || !authAdapter.includes('context.expiresAt')) {
  throw new Error('Delegated auth must be portal-integrated only and expiry-aware.');
}

const portalModels = readFileSync(join(root, 'src/app/core/portal-shell/portal-shell.models.ts'), 'utf8');
for (const token of ['PortalShellContext', 'PortalFeatureFlags', 'PORTAL_SHELL_CONTEXT', '__PORTAL_SHELL_CONTEXT__']) {
  if (!portalModels.includes(token)) throw new Error(`Portal shell contract is missing ${token}.`);
}
for (const token of ['SUPPORTED_PORTAL_SHELL_CONTRACT_VERSION', 'contractVersion', 'source', 'issuedAt', 'expiresAt', 'capabilities', 'warnings']) {
  if (!portalModels.includes(token)) throw new Error(`Portal shell contract hardening missing ${token}.`);
}

const portalDefaults = readFileSync(join(root, 'src/app/core/portal-shell/portal-shell.defaults.ts'), 'utf8');
for (const safeFlag of ['allowXmlPreviewUi: false', 'allowMutations: false', 'allowDevHeaders: false']) {
  if (!portalDefaults.includes(safeFlag)) throw new Error(`Safe feature flag default missing: ${safeFlag}.`);
}

for (const safeFlag of ['allowPurchaseCommands: false', 'allowVoidedDocumentCommands: false', 'allowExternalApprovalCommands: false', 'allowEvidenceReferenceMetadata: false', 'allowApprovalDecisionFoundation: false', 'allowPortalContentFileEvidenceReferences: false', 'allowPortalNotificationIntents: false', 'allowNotificationSend: false', 'allowEvidenceUpload: false', 'allowProductizationReadiness: true', 'allowProductiveActivation: false', 'allowOfficialTaxFlows: false', 'allowAtsOfficialActions: false', 'allowSriSubmission: false']) {
  if (!portalDefaults.includes(safeFlag)) throw new Error(`Command safety flag default missing: ${safeFlag}.`);
}

const prodEnvironment = readFileSync(join(root, 'src/environments/environment.ts'), 'utf8');
for (const token of ['production: true', 'enableDevHeaders: false', 'allowXmlPreview: false']) {
  if (!prodEnvironment.includes(token)) throw new Error(`Production environment must keep ${token}.`);
}
if (prodEnvironment.includes('financial.electronicdocuments.manage')) {
  throw new Error('Production environment must not include development permissions.');
}

const commandGuard = readFileSync(join(root, 'src/app/core/services/command-guard.service.ts'), 'utf8');
for (const token of ['allowMutations', 'allowPurchaseCommands', 'allowVoidedDocumentCommands', 'allowExternalApprovalCommands', 'allowEvidenceReferenceMetadata', 'allowApprovalDecisionFoundation', 'allowPortalContentFileEvidenceReferences', 'allowPortalNotificationIntents', 'allowNotificationSend', 'allowEvidenceUpload', 'allowProductiveActivation', 'allowOfficialTaxFlows', 'financial.electronicdocuments.manage']) {
  if (!commandGuard.includes(token)) throw new Error(`Command guard missing ${token}.`);
}

const purchaseApi = readFileSync(join(root, 'src/app/core/services/purchase-tax-document-api.service.ts'), 'utf8');
const voidedApi = readFileSync(join(root, 'src/app/core/services/voided-tax-document-api.service.ts'), 'utf8');
for (const text of [purchaseApi, voidedApi]) {
  if (!text.includes('productization-readiness')) throw new Error('Productization readiness route missing from tax document API service.');
}

const portalE2eApi = readFileSync(join(root, 'src/app/core/services/portal-integration-readiness-api.service.ts'), 'utf8');
if (!portalE2eApi.includes('/api/financial/portal-integration/readiness')) {
  throw new Error('Portal E2E readiness API route missing.');
}

const externalApproval = readFileSync(join(root, 'src/app/core/services/external-approval-api.service.ts'), 'utf8');
if (!externalApproval.includes('/api/financial/external-approvals/readiness')) {
  throw new Error('External approval readiness must use the real backend route.');
}
for (const token of ['/api/financial/external-approval-requests', 'integration-readiness', 'evidence-references', 'decision']) {
  if (!externalApproval.includes(token)) throw new Error(`External approval persisted workflow API missing ${token}.`);
}

const externalApprovalComponent = readFileSync(join(root, 'src/app/features/external-approvals/external-approvals.component.ts'), 'utf8');
for (const token of ['Portal integration readiness', 'reference only / Portal-owned evidence', 'No upload', 'Sin envío de notificaciones', 'ApprovedFoundation no habilita producción', 'Evidence reference is Portal-owned metadata only', 'Notification intent is prepared only; no send', 'External approval does not replace legal/tax approval', 'Production requires Portal + legal/tax/security approval', 'No file stored in Financiero']) {
  if (!externalApprovalComponent.includes(token)) throw new Error(`External approval UI boundary missing ${token}.`);
}

for (const screen of ['src/app/features/purchases/purchases.component.ts', 'src/app/features/voided-documents/voided-documents.component.ts']) {
  const text = readFileSync(join(root, screen), 'utf8');
  for (const token of ['Productization readiness', 'Producción bloqueada', 'Sin upload', 'Sin notification send']) {
    if (!text.includes(token)) throw new Error(`${screen} missing productization readiness token ${token}.`);
  }
  for (const forbiddenControl of ['type="file"', '<button type="button">Producción', '<button type="button">Enviar SRI', '<button type="button">ATS oficial', 'sendNotification', 'downloadEvidence']) {
    if (text.includes(forbiddenControl)) throw new Error(`${screen} must not expose productive control ${forbiddenControl}.`);
  }
}
for (const forbiddenControl of ['type="file"', 'Upload disabled', 'Notify disabled', 'sendNotification', 'downloadEvidence']) {
  if (externalApprovalComponent.includes(forbiddenControl)) throw new Error(`External approval UI must not expose ${forbiddenControl}.`);
}

const validation = readFileSync(join(root, 'src/app/core/portal-shell/portal-shell-validation.ts'), 'utf8');
for (const token of ['sanitizeContractVersion', 'sanitizeMenuItems', 'sanitizeDelegatedToken', 'Portal menu route rejected', 'Portal integrated context missing required user']) {
  if (!validation.includes(token)) throw new Error(`Portal context validation hardening missing ${token}.`);
}

const appComponent = readFileSync(join(root, 'src/app/app.component.ts'), 'utf8');
for (const token of ['missingRequiredPortalContext()', 'hasUnsupportedContract()', 'fin-portal-context-required']) {
  if (!appComponent.includes(token)) throw new Error(`Production Portal context block missing ${token}.`);
}

const dashboard = readFileSync(join(root, 'src/app/features/dashboard/dashboard.component.ts'), 'utf8');
for (const token of ['Portal E2E readiness', 'development standalone only', 'production requires Portal context']) {
  if (!dashboard.includes(token)) throw new Error(`Dashboard missing Portal E2E readiness token ${token}.`);
}

const menuAdapter = readFileSync(join(root, 'src/app/core/adapters/portal-menu.adapter.ts'), 'utf8');
for (const token of ['hasPermission', "item.route.startsWith('/')", "!item.route.startsWith('//')"]) {
  if (!menuAdapter.includes(token)) throw new Error(`Portal menu hardening missing ${token}.`);
}

for (const doc of [
  'docs/releases/financial-sprint-06-closure.md',
  'docs/releases/financial-sprint-06-release-notes.md',
  'docs/qa/financial-sprint-06-qa-evidence.md',
  'docs/architecture/financial-sprint-06-architecture-snapshot.md',
  'docs/coordination/financial-sprint-06-p5-closure-ux-portal-readiness.md',
  'docs/coordination/financial-sprint-07-p1-real-portal-shell-contract.md',
  'docs/architecture/decisions/adr-023-real-portal-shell-contract-hardening.md',
  'docs/coordination/financial-sprint-07-p2-external-approval-persistence.md',
  'docs/architecture/decisions/adr-024-external-approval-workflow-persistence.md',
  'docs/coordination/financial-sprint-07-p3-contentfile-notification-boundary.md',
  'docs/architecture/decisions/adr-025-portal-contentfile-notification-boundary.md',
  'docs/integration/portal-contentfile-notification-boundary.md',
  'docs/coordination/financial-sprint-07-p4-controlled-productization-readiness.md',
  'docs/architecture/decisions/adr-026-controlled-productization-readiness.md',
  'docs/releases/financial-sprint-07-readiness-notes.md',
  'docs/coordination/financial-sprint-07-closure.md',
  'docs/qa/financial-sprint-07-qa-evidence.md',
  'docs/architecture/financial-sprint-07-capability-matrix.md',
  'docs/security/financial-sprint-07-security-checklist.md',
  'docs/roadmap/financial-sprint-08-roadmap.md',
  'docs/releases/financial-sprint-07-release-notes.md',
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
  'docs/qa/financial-sprint-10-p2-owner-evidence-review.md',
  'docs/qa/financial-sprint-10-p2-acceptance-gate-execution.md',
  'docs/coordination/financial-sprint-10-p3-owner-escalation-matrix.md',
  'docs/coordination/financial-sprint-10-p3-formal-evidence-request.md',
  'docs/qa/financial-sprint-10-p3-external-remediation-log.md',
  'docs/qa/financial-sprint-10-p4-remediation-followup-evidence.md',
  'docs/coordination/financial-sprint-10-p4-executive-block-decision.md',
  'docs/coordination/financial-sprint-10-closure.md',
  'docs/qa/financial-sprint-10-final-evidence.md',
  'docs/releases/financial-sprint-10-release-notes.md',
  'docs/roadmap/financial-sprint-11-decision-matrix.md',
  'docs/coordination/financial-sprint-11-p1-external-remediation-plan.md',
  'docs/coordination/financial-sprint-11-p1-external-repo-handoff-checklist.md',
  'docs/qa/financial-sprint-11-p1-return-to-pass-criteria.md',
  'docs/releases/financial-sprint-11-notes.md',
  'docs/qa/financial-sprint-11-p2-external-evidence-followup.md',
  'docs/qa/financial-sprint-11-p2-return-to-pass-review.md',
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
  'docs/releases/financial-sprint-09-release-notes.md',
  'tools/validate-portal-financiero-e2e.ps1',
  'docs/frontend/portal-shell-readiness-matrix.md',
  'docs/frontend/portal-shell-contract.md'
]) {
  statSync(join(repoRoot, doc));
}

for (const doc of [
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
  'docs/qa/financial-sprint-10-p2-owner-evidence-review.md',
  'docs/qa/financial-sprint-10-p2-acceptance-gate-execution.md',
  'docs/coordination/financial-sprint-10-p3-owner-escalation-matrix.md',
  'docs/coordination/financial-sprint-10-p3-formal-evidence-request.md',
  'docs/qa/financial-sprint-10-p3-external-remediation-log.md',
  'docs/qa/financial-sprint-10-p4-remediation-followup-evidence.md',
  'docs/coordination/financial-sprint-10-p4-executive-block-decision.md',
  'docs/coordination/financial-sprint-10-closure.md',
  'docs/qa/financial-sprint-10-final-evidence.md',
  'docs/releases/financial-sprint-10-release-notes.md',
  'docs/roadmap/financial-sprint-11-decision-matrix.md',
  'docs/coordination/financial-sprint-11-p1-external-remediation-plan.md',
  'docs/coordination/financial-sprint-11-p1-external-repo-handoff-checklist.md',
  'docs/qa/financial-sprint-11-p1-return-to-pass-criteria.md',
  'docs/releases/financial-sprint-11-notes.md',
  'docs/qa/financial-sprint-11-p2-external-evidence-followup.md',
  'docs/qa/financial-sprint-11-p2-return-to-pass-review.md',
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
  'docs/releases/financial-sprint-08-release-notes.md',
  'docs/releases/financial-sprint-09-release-notes.md'
]) {
  const text = readFileSync(join(repoRoot, doc), 'utf8');
  for (const token of ['BLOCKED_DEPENDENCY', 'No SRI Production', 'No official ATS', 'No legal-final RIDE', 'No productive XAdES']) {
    if (!text.includes(token)) throw new Error(`${doc} missing Sprint 8 P5 closure token ${token}.`);
  }
}

for (const doc of [
  'docs/coordination/financial-sprint-09-closure.md',
  'docs/qa/financial-sprint-09-final-infra-evidence.md',
  'docs/roadmap/financial-sprint-10-decision-matrix.md',
  'docs/roadmap/financial-external-dependency-backlog.md',
  'docs/releases/financial-sprint-09-release-notes.md'
]) {
  const text = readFileSync(join(repoRoot, doc), 'utf8');
  for (const token of ['Portal Gateway', 'shared SQL', 'not production-ready', 'No SRI Production', 'No official ATS', 'No legal-final RIDE', 'No productive XAdES']) {
    if (!text.includes(token)) throw new Error(`${doc} missing Sprint 9 P5 closure token ${token}.`);
  }
}

const preflight = readFileSync(join(repoRoot, 'tools/validate-portal-financiero-e2e.ps1'), 'utf8');
for (const token of ['PortalGatewayHealthPath', 'PortalShellHealthPath', 'FinancialApiHealthPath', 'EvidenceOutputPath', 'AcceptanceGateReport', 'OwnerEvidenceRequired', 'AcceptanceGateSummary', 'HEALTH_PATH_NOT_CONFIRMED', 'HEALTH_ROUTE_ALTERNATIVE_REQUIRED', 'HTTP_STATUS_UNEXPECTED']) {
  if (!preflight.includes(token)) throw new Error(`Sprint 9 P4 preflight health route configuration missing ${token}.`);
}

for (const doc of [
  'docs/coordination/financial-sprint-10-p1-owner-evidence-intake.md',
  'docs/qa/financial-sprint-10-p1-e2e-acceptance-gate.md',
  'docs/qa/financial-sprint-10-p2-owner-evidence-review.md',
  'docs/qa/financial-sprint-10-p2-acceptance-gate-execution.md',
  'docs/coordination/financial-sprint-10-p3-owner-escalation-matrix.md',
  'docs/coordination/financial-sprint-10-p3-formal-evidence-request.md',
  'docs/qa/financial-sprint-10-p3-external-remediation-log.md',
  'docs/qa/financial-sprint-10-p4-remediation-followup-evidence.md',
  'docs/coordination/financial-sprint-10-p4-executive-block-decision.md',
  'docs/coordination/financial-sprint-10-closure.md',
  'docs/qa/financial-sprint-10-final-evidence.md',
  'docs/releases/financial-sprint-10-release-notes.md',
  'docs/roadmap/financial-sprint-11-decision-matrix.md',
  'docs/coordination/financial-sprint-11-p1-external-remediation-plan.md',
  'docs/coordination/financial-sprint-11-p1-external-repo-handoff-checklist.md',
  'docs/qa/financial-sprint-11-p1-return-to-pass-criteria.md',
  'docs/releases/financial-sprint-11-notes.md',
  'docs/qa/financial-sprint-11-p2-external-evidence-followup.md',
  'docs/qa/financial-sprint-11-p2-return-to-pass-review.md',
  'docs/qa/templates/sql-common-evidence-template.md',
  'docs/qa/templates/portal-gateway-evidence-template.md',
  'docs/qa/templates/portal-shell-evidence-template.md',
  'docs/qa/templates/portal-contract-evidence-template.md'
]) {
  const text = readFileSync(join(repoRoot, doc), 'utf8');
  for (const token of ['BLOCKED_DEPENDENCY', 'not production-ready', 'No SRI Production', 'No official ATS', 'No legal-final RIDE', 'No productive XAdES']) {
    if (!text.includes(token)) throw new Error(`${doc} missing Sprint 10 P1 no-production token ${token}.`);
  }
}

for (const doc of [
  'docs/qa/financial-sprint-10-p2-owner-evidence-review.md',
  'docs/qa/financial-sprint-10-p2-acceptance-gate-execution.md'
]) {
  const text = readFileSync(join(repoRoot, doc), 'utf8');
  for (const token of ['NotReceived', 'EvidencePending', 'BLOCKED_DEPENDENCY', 'Portal Gateway', 'shared SQL', 'not production-ready', 'No SRI Production', 'No official ATS', 'No legal-final RIDE', 'No productive XAdES']) {
    if (!text.includes(token)) throw new Error(`${doc} missing Sprint 10 P2 evidence review token ${token}.`);
  }
}

for (const doc of [
  'docs/coordination/financial-sprint-10-p3-owner-escalation-matrix.md',
  'docs/coordination/financial-sprint-10-p3-formal-evidence-request.md',
  'docs/qa/financial-sprint-10-p3-external-remediation-log.md',
  'docs/coordination/financial-sprint-10-p1-owner-evidence-intake.md',
  'docs/roadmap/financial-external-dependency-backlog.md',
  'docs/architecture/financial-risk-register.md',
  'docs/roadmap/financial-sprint-10-decision-matrix.md'
]) {
  const text = readFileSync(join(repoRoot, doc), 'utf8');
  for (const token of ['EvidencePending', 'BLOCKED_DEPENDENCY', 'Portal Gateway', 'shared SQL', 'not production-ready', 'SLA', 'No SRI Production', 'No official ATS', 'No legal-final RIDE', 'No productive XAdES']) {
    if (!text.includes(token)) throw new Error(`${doc} missing Sprint 10 P3 escalation token ${token}.`);
  }
}

for (const doc of [
  'docs/qa/financial-sprint-10-p4-remediation-followup-evidence.md',
  'docs/coordination/financial-sprint-10-p4-executive-block-decision.md',
  'docs/coordination/financial-sprint-10-p3-owner-escalation-matrix.md',
  'docs/qa/financial-sprint-10-p3-external-remediation-log.md',
  'docs/coordination/financial-sprint-10-p1-owner-evidence-intake.md',
  'docs/roadmap/financial-external-dependency-backlog.md',
  'docs/architecture/financial-risk-register.md',
  'docs/roadmap/financial-sprint-10-decision-matrix.md'
]) {
  const text = readFileSync(join(repoRoot, doc), 'utf8');
  for (const token of ['NoResponse', 'EvidencePending', 'BLOCKED_DEPENDENCY', 'executive', 'Portal Gateway', 'shared SQL', 'not production-ready', 'No SRI Production', 'No official ATS', 'No legal-final RIDE', 'No productive XAdES']) {
    if (!text.includes(token)) throw new Error(`${doc} missing Sprint 10 P4 executive block token ${token}.`);
  }
}

for (const doc of [
  'docs/coordination/financial-sprint-10-closure.md',
  'docs/qa/financial-sprint-10-final-evidence.md',
  'docs/releases/financial-sprint-10-release-notes.md',
  'docs/roadmap/financial-sprint-11-decision-matrix.md',
  'docs/roadmap/financial-external-dependency-backlog.md',
  'docs/architecture/financial-risk-register.md',
  'docs/roadmap/financial-sprint-10-decision-matrix.md'
]) {
  const text = readFileSync(join(repoRoot, doc), 'utf8');
  for (const token of ['BLOCKED_DEPENDENCY', 'not production-ready', 'Sprint 11', 'shared SQL', 'Portal Gateway', 'No SRI Production', 'No official ATS', 'No legal-final RIDE', 'No productive XAdES']) {
    if (!text.includes(token)) throw new Error(`${doc} missing Sprint 10 P5 closure token ${token}.`);
  }
}

for (const doc of [
  'docs/coordination/financial-sprint-11-p1-external-remediation-plan.md',
  'docs/coordination/financial-sprint-11-p1-external-repo-handoff-checklist.md',
  'docs/qa/financial-sprint-11-p1-return-to-pass-criteria.md',
  'docs/releases/financial-sprint-11-notes.md',
  'docs/roadmap/financial-external-dependency-backlog.md',
  'docs/architecture/financial-risk-register.md',
  'docs/roadmap/financial-sprint-11-decision-matrix.md'
]) {
  const text = readFileSync(join(repoRoot, doc), 'utf8');
  for (const token of ['BLOCKED_DEPENDENCY', 'external', 'outside', 'shared SQL', 'Portal Gateway', 'SCRIPT_EXIT=0', 'not production-ready', 'No SRI Production', 'No official ATS', 'No legal-final RIDE', 'No productive XAdES']) {
    if (!text.includes(token)) throw new Error(`${doc} missing Sprint 11 P1 remediation token ${token}.`);
  }
}

for (const doc of [
  'docs/qa/financial-sprint-11-p2-external-evidence-followup.md',
  'docs/qa/financial-sprint-11-p2-return-to-pass-review.md',
  'docs/coordination/financial-sprint-11-p1-external-repo-handoff-checklist.md',
  'docs/coordination/financial-sprint-11-p1-external-remediation-plan.md',
  'docs/roadmap/financial-external-dependency-backlog.md',
  'docs/architecture/financial-risk-register.md',
  'docs/roadmap/financial-sprint-11-decision-matrix.md',
  'docs/releases/financial-sprint-11-notes.md'
]) {
  const text = readFileSync(join(repoRoot, doc), 'utf8');
  for (const token of ['NoResponse', 'EvidencePending', 'BLOCKED_DEPENDENCY', 'Portal Gateway', 'shared SQL', 'SCRIPT_EXIT=0', 'not production-ready', 'No SRI Production', 'No official ATS', 'No legal-final RIDE', 'No productive XAdES']) {
    if (!text.includes(token)) throw new Error(`${doc} missing Sprint 11 P2 evidence follow-up token ${token}.`);
  }
}

for (const doc of [
  'docs/qa/financial-sprint-11-p3-gate-decision-evidence.md',
  'docs/coordination/financial-sprint-11-p3-external-escalation.md',
  'docs/coordination/financial-sprint-10-p1-owner-evidence-intake.md',
  'docs/coordination/financial-sprint-11-p1-external-repo-handoff-checklist.md',
  'docs/coordination/financial-sprint-11-p1-external-remediation-plan.md',
  'docs/roadmap/financial-external-dependency-backlog.md',
  'docs/architecture/financial-risk-register.md',
  'docs/roadmap/financial-sprint-11-decision-matrix.md',
  'docs/releases/financial-sprint-11-notes.md',
  'docs/roadmap/financial-controlled-productization-backlog.md'
]) {
  const text = readFileSync(join(repoRoot, doc), 'utf8');
  for (const token of ['NoResponse', 'EvidencePending', 'BLOCKED_DEPENDENCY', 'Portal Gateway', 'shared SQL', 'SCRIPT_EXIT=2', 'SCRIPT_EXIT=0', 'not production-ready', 'No SRI Production', 'No official ATS', 'No legal-final RIDE', 'No productive XAdES']) {
    if (!text.includes(token)) throw new Error(`${doc} missing Sprint 11 P3 gate decision token ${token}.`);
  }
}

for (const doc of [
  'docs/qa/financial-sprint-11-p4-execution-evidence.md',
  'docs/coordination/financial-sprint-11-p4-external-escalation-followup.md',
  'docs/coordination/financial-sprint-10-p1-owner-evidence-intake.md',
  'docs/coordination/financial-sprint-11-p1-external-repo-handoff-checklist.md',
  'docs/coordination/financial-sprint-11-p1-external-remediation-plan.md',
  'docs/coordination/financial-sprint-11-p3-external-escalation.md',
  'docs/roadmap/financial-external-dependency-backlog.md',
  'docs/architecture/financial-risk-register.md',
  'docs/roadmap/financial-sprint-11-decision-matrix.md',
  'docs/releases/financial-sprint-11-notes.md',
  'docs/roadmap/financial-controlled-productization-backlog.md'
]) {
  const text = readFileSync(join(repoRoot, doc), 'utf8');
  for (const token of ['NoResponse', 'EvidencePending', 'BLOCKED_DEPENDENCY', 'Portal Gateway', 'shared SQL', 'SCRIPT_EXIT=2', 'SCRIPT_EXIT=0', 'PASS E2E real', 'NOT_READY', 'not production-ready', 'No SRI Production', 'No official ATS', 'No legal-final RIDE', 'No productive XAdES']) {
    if (!text.includes(token)) throw new Error(`${doc} missing Sprint 11 P4 execution token ${token}.`);
  }
}

for (const doc of [
  'docs/coordination/financial-sprint-11-closure.md',
  'docs/qa/financial-sprint-11-final-evidence.md',
  'docs/releases/financial-sprint-11-notes.md',
  'docs/roadmap/financial-next-cycle-decision-matrix.md',
  'docs/roadmap/financial-external-dependency-backlog.md',
  'docs/roadmap/financial-controlled-productization-backlog.md',
  'docs/architecture/financial-risk-register.md',
  'docs/roadmap/financial-sprint-11-decision-matrix.md'
]) {
  const text = readFileSync(join(repoRoot, doc), 'utf8');
  for (const token of ['Sprint 11', 'BLOCKED_DEPENDENCY', 'NoResponse', 'EvidencePending', 'SCRIPT_EXIT=2', 'SCRIPT_EXIT=0', 'PASS E2E real', 'NOT_READY', 'not production-ready', 'No SRI Production', 'No official ATS', 'No legal-final RIDE', 'No productive XAdES']) {
    if (!text.includes(token)) throw new Error(`${doc} missing Sprint 11 P5 controlled closure token ${token}.`);
  }
}

for (const doc of [
  'docs/coordination/financial-sprint-07-closure.md',
  'docs/qa/financial-sprint-07-qa-evidence.md',
  'docs/security/financial-sprint-07-security-checklist.md',
  'docs/roadmap/financial-sprint-08-roadmap.md',
  'docs/releases/financial-sprint-07-release-notes.md'
]) {
  const text = readFileSync(join(repoRoot, doc), 'utf8');
  for (const token of ['No SRI Production', 'No official ATS', 'No legal-final RIDE', 'No productive XAdES']) {
    if (!text.includes(token)) throw new Error(`${doc} missing no-production token ${token}.`);
  }
}

const foundationDisclaimer = readFileSync(join(root, 'src/app/shared/components/foundation-disclaimer.component.ts'), 'utf8');
if (!foundationDisclaimer.includes('Foundation / No productivo')) {
  throw new Error('Foundation disclaimer must remain explicit.');
}

console.log('Frontend foundation checks passed.');
