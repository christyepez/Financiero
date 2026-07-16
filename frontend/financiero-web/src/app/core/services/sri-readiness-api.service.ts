import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ReadinessResponse } from './api.models';

@Injectable({ providedIn: 'root' })
export class SriReadinessApiService {
  private readonly api = inject(ApiService);

  getReadiness(): Observable<ReadinessResponse> {
    return this.api.get<ReadinessResponse>('/api/financial/electronic-documents/sri/readiness');
  }
}
