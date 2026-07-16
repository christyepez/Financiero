import { Routes } from '@angular/router';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { SriReadinessComponent } from './features/sri-readiness/sri-readiness.component';
import { AtsReadinessComponent } from './features/ats-readiness/ats-readiness.component';
import { ExternalApprovalsComponent } from './features/external-approvals/external-approvals.component';
import { TaxCatalogsComponent } from './features/tax-catalogs/tax-catalogs.component';
import { PurchasesComponent } from './features/purchases/purchases.component';
import { VoidedDocumentsComponent } from './features/voided-documents/voided-documents.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'sri-readiness', component: SriReadinessComponent },
  { path: 'ats-readiness', component: AtsReadinessComponent },
  { path: 'external-approvals', component: ExternalApprovalsComponent },
  { path: 'tax-catalogs', component: TaxCatalogsComponent },
  { path: 'purchases', component: PurchasesComponent },
  { path: 'voided-documents', component: VoidedDocumentsComponent },
  { path: '**', redirectTo: 'dashboard' }
];
