import { Injectable, signal } from '@angular/core';

export interface PortalMenuItem {
  label: string;
  path: string;
  permission: string;
}

@Injectable({ providedIn: 'root' })
export class PortalMenuAdapter {
  readonly items = signal<PortalMenuItem[]>([
    { label: 'Dashboard', path: '/dashboard', permission: 'financial.electronicdocuments.read' },
    { label: 'SRI readiness', path: '/sri-readiness', permission: 'financial.electronicdocuments.manage' },
    { label: 'ATS readiness', path: '/ats-readiness', permission: 'financial.electronicdocuments.read' },
    { label: 'Aprobaciones externas', path: '/external-approvals', permission: 'financial.electronicdocuments.manage' },
    { label: 'Catálogos tributarios', path: '/tax-catalogs', permission: 'financial.electronicdocuments.read' },
    { label: 'Compras', path: '/purchases', permission: 'financial.electronicdocuments.read' },
    { label: 'Anulados', path: '/voided-documents', permission: 'financial.electronicdocuments.read' }
  ]);

  currentTitle(): string {
    return 'Financiero Shell';
  }
}
