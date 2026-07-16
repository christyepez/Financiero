import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PortalAuthAdapter {
  getAccessToken(): string | null {
    return null;
  }

  getDevelopmentPermissions(): string | null {
    return environment.enableDevHeaders ? environment.devPermissionsHeader : null;
  }
}
