import { Injectable, inject } from '@angular/core';
import { PortalAuthAdapter } from '../adapters/portal-auth.adapter';
import { PortalConfigurationAdapter } from '../adapters/portal-configuration.adapter';

@Injectable({ providedIn: 'root' })
export class CommandGuardService {
  private readonly configuration = inject(PortalConfigurationAdapter);
  private readonly auth = inject(PortalAuthAdapter);

  canRunPurchaseCommands(): boolean {
    const flags = this.configuration.featureFlags();
    return flags.allowMutations && flags.allowPurchaseCommands && this.hasManagePermission();
  }

  canRunVoidedDocumentCommands(): boolean {
    const flags = this.configuration.featureFlags();
    return flags.allowMutations && flags.allowVoidedDocumentCommands && this.hasManagePermission();
  }

  canRunExternalApprovalCommands(): boolean {
    const flags = this.configuration.featureFlags();
    return flags.allowMutations && flags.allowExternalApprovalCommands && this.hasManagePermission();
  }

  canRunEvidenceReferenceCommands(): boolean {
    const flags = this.configuration.featureFlags();
    return this.canRunExternalApprovalCommands() && flags.allowEvidenceReferenceMetadata && flags.allowPortalContentFileEvidenceReferences && !flags.allowEvidenceUpload;
  }

  canRunApprovalDecisionCommands(): boolean {
    const flags = this.configuration.featureFlags();
    return this.canRunExternalApprovalCommands() && flags.allowApprovalDecisionFoundation;
  }

  canPrepareNotificationIntents(): boolean {
    const flags = this.configuration.featureFlags();
    return this.canRunExternalApprovalCommands() && flags.allowPortalNotificationIntents && !flags.allowNotificationSend;
  }

  canUploadEvidence(): boolean {
    return false;
  }

  canSendNotifications(): boolean {
    return false;
  }

  canActivateProduction(): boolean {
    const flags = this.configuration.featureFlags();
    return false && flags.allowProductiveActivation && flags.allowOfficialTaxFlows;
  }

  disabledReason(kind: 'purchase' | 'voided' | 'approval'): string {
    const flags = this.configuration.featureFlags();
    if (!flags.allowMutations) return 'Comandos deshabilitados por feature flag allowMutations=false.';
    if (kind === 'purchase' && !flags.allowPurchaseCommands) return 'Comandos de compras deshabilitados por feature flag.';
    if (kind === 'voided' && !flags.allowVoidedDocumentCommands) return 'Comandos de anulados deshabilitados por feature flag.';
    if (kind === 'approval' && !flags.allowExternalApprovalCommands) return 'Comandos de aprobaciones externas deshabilitados por feature flag.';
    if (!this.hasManagePermission()) return 'Permiso requerido: financial.electronicdocuments.manage.';
    return 'Comando no disponible en este contexto foundation.';
  }

  private hasManagePermission(): boolean {
    return this.auth.permissions().includes('financial.electronicdocuments.manage');
  }
}
