import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiEnvelope } from './api.models';
import { PortalConfigurationAdapter } from '../adapters/portal-configuration.adapter';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly configuration = inject(PortalConfigurationAdapter);

  get<T>(path: string, params?: Record<string, string | number | boolean>): Observable<T> {
    return this.http.get<ApiEnvelope<T> | T>(`${this.baseUrl()}${path}`, {
      params: this.params(params)
    }).pipe(map(response => this.unwrap(response)));
  }

  post<T>(path: string, body: unknown, params?: Record<string, string | number | boolean>): Observable<T> {
    return this.http.post<ApiEnvelope<T> | T>(`${this.baseUrl()}${path}`, body, {
      params: this.params(params)
    }).pipe(map(response => this.unwrap(response)));
  }

  private params(values?: Record<string, string | number | boolean>): HttpParams {
    let params = new HttpParams();
    Object.entries(values ?? {}).forEach(([key, value]) => {
      params = params.set(key, String(value));
    });
    return params;
  }

  private unwrap<T>(response: ApiEnvelope<T> | T): T {
    if (response && typeof response === 'object' && ('data' in response || 'error' in response)) {
      const envelope = response as ApiEnvelope<T>;
      if (envelope.error) {
        throw new Error(envelope.error.message || envelope.error.code || 'Financial API request failed.');
      }
      return envelope.data as T;
    }

    return response as T;
  }

  private baseUrl(): string {
    return (this.configuration.apiBaseUrl() || environment.apiBaseUrl).replace(/\/$/, '');
  }
}
