import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { PortalIntegrationReadiness } from './api.models';

@Injectable({ providedIn: 'root' })
export class PortalIntegrationReadinessApiService {
  private readonly api = inject(ApiService);

  getReadiness() {
    return this.api.get<PortalIntegrationReadiness>('/api/financial/portal-integration/readiness');
  }
}
