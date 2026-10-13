import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  CreateFinancialAccount,
  FinancialAccount,
  FiscalPeriod,
  FiscalYear,
  JournalEntry,
  PageResponse
} from './api.models';

@Injectable({ providedIn: 'root' })
export class AccountingApiService {
  private readonly api = inject(ApiService);

  getAccounts(search = ''): Observable<PageResponse<FinancialAccount>> {
    return this.api.get<PageResponse<FinancialAccount>>('/financial/accounts/', {
      search,
      page: 1,
      pageSize: 200
    });
  }

  createAccount(request: CreateFinancialAccount): Observable<FinancialAccount> {
    return this.api.post<FinancialAccount>('/financial/accounts/', request);
  }

  getFiscalYears(): Observable<PageResponse<FiscalYear>> {
    return this.api.get<PageResponse<FiscalYear>>('/financial/fiscal-years/', { page: 1, pageSize: 100 });
  }

  getFiscalPeriods(): Observable<PageResponse<FiscalPeriod>> {
    return this.api.get<PageResponse<FiscalPeriod>>('/financial/fiscal-periods/', { page: 1, pageSize: 200 });
  }

  getJournalEntries(search = ''): Observable<PageResponse<JournalEntry>> {
    return this.api.get<PageResponse<JournalEntry>>('/financial/journal-entries/', {
      search,
      page: 1,
      pageSize: 200
    });
  }
}
