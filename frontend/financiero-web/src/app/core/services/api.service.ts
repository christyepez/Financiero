import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl.replace(/\/$/, '');

  get<T>(path: string, params?: Record<string, string | number | boolean>): Observable<T> {
    return this.http.get<T>(`${this.baseUrl}${path}`, {
      params: this.params(params)
    });
  }

  post<T>(path: string, body: unknown, params?: Record<string, string | number | boolean>): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}${path}`, body, {
      params: this.params(params)
    });
  }

  private params(values?: Record<string, string | number | boolean>): HttpParams {
    let params = new HttpParams();
    Object.entries(values ?? {}).forEach(([key, value]) => {
      params = params.set(key, String(value));
    });
    return params;
  }
}
