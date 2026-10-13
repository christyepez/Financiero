import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import { ErrorMessageComponent } from '../../shared/components/error-message.component';
import { LoadingStateComponent } from '../../shared/components/loading-state.component';

type FinancialOperationsArea = 'electronic-documents' | 'tax-reporting' | 'tax-action-queue' | 'tax-legal-review';

interface OperationsDefinition {
  title: string;
  description: string;
  endpoint: string;
  periodAware: boolean;
  statusAware: boolean;
}

const definitions: Record<FinancialOperationsArea, OperationsDefinition> = {
  'electronic-documents': {
    title: 'Documentos electrónicos',
    description: 'Consulta documentos electrónicos y sus estados operativos. Las acciones oficiales SRI permanecen gobernadas por permisos y feature flags.',
    endpoint: '/api/financial/electronic-documents/',
    periodAware: false,
    statusAware: true
  },
  'tax-reporting': {
    title: 'Reporting tributario',
    description: 'Consulta documentos tributarios consolidados para análisis y control del período.',
    endpoint: '/api/financial/tax-reporting/documents',
    periodAware: true,
    statusAware: true
  },
  'tax-action-queue': {
    title: 'Cola tributaria',
    description: 'Revisa pendientes y acciones requeridas sobre documentos tributarios del período.',
    endpoint: '/api/financial/tax-reporting/action-queue',
    periodAware: true,
    statusAware: false
  },
  'tax-legal-review': {
    title: 'Revisión tributaria',
    description: 'Consulta el checklist de aprobación legal tributaria. Esta vista es informativa y no habilita flujos oficiales.',
    endpoint: '/api/financial/tax-legal-review/approval-checklist',
    periodAware: false,
    statusAware: false
  }
};

@Component({
  standalone: true,
  selector: 'fin-financial-operations',
  imports: [ErrorMessageComponent, LoadingStateComponent],
  template: `
    <section class="panel">
      <div class="workspace-heading">
        <div>
          <p class="eyebrow">Financiero · Operación</p>
          <h2>{{ definition().title }}</h2>
          <p class="muted">{{ definition().description }}</p>
        </div>
        <button type="button" (click)="load()" [disabled]="loading()">Actualizar</button>
      </div>

      <div class="period-selector">
        @if (definition().periodAware) {
          <label>Período
            <input type="month"
                   [value]="period()"
                   (input)="period.set($any($event.target).value)">
          </label>
        }
        @if (definition().statusAware) {
          <label>Estado
            <input type="search"
                   [value]="status()"
                   (input)="status.set($any($event.target).value)"
                   placeholder="Draft, Generated, Authorized...">
          </label>
        }
        <button type="button" (click)="load()" [disabled]="loading()">Consultar</button>
      </div>

      @if (summary().length) {
        <div class="accounting-summary operations-summary">
          @for (item of summary(); track item.label) {
            <article>
              <strong>{{ item.value }}</strong>
              <span>{{ item.label }}</span>
            </article>
          }
        </div>
      }
    </section>

    <fin-loading-state [loading]="loading()" />
    <fin-error-message [message]="error()" />

    @if (!loading()) {
      <section class="panel accounting-grid-panel">
        <div class="panel-heading">
          <div>
            <h2>{{ rows().length }} registros</h2>
            <p class="muted">Los datos provienen del API Financiero a través del Gateway del Portal.</p>
          </div>
          <span class="badge info">Solo lectura</span>
        </div>

        @if (rows().length) {
          <div class="table-scroll">
            <table class="table accounting-grid">
              <thead>
                <tr>
                  @for (column of columns(); track column) {
                    <th>{{ label(column) }}</th>
                  }
                </tr>
              </thead>
              <tbody>
                @for (row of rows(); track row['id'] ?? $index) {
                  <tr>
                    @for (column of columns(); track column) {
                      <td>
                        @if (column.toLowerCase().includes('status')) {
                          <span class="badge info">{{ cell(row, column) }}</span>
                        } @else {
                          {{ cell(row, column) }}
                        }
                      </td>
                    }
                  </tr>
                }
              </tbody>
            </table>
          </div>
        } @else {
          <p class="muted">No hay registros para los filtros actuales.</p>
        }
      </section>
    }
  `
})
export class FinancialOperationsComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(ApiService);
  private routeSubscription?: Subscription;

  protected readonly area = signal<FinancialOperationsArea>('electronic-documents');
  protected readonly period = signal(currentPeriod());
  protected readonly status = signal('');
  protected readonly rows = signal<Record<string, unknown>[]>([]);
  protected readonly columns = signal<string[]>([]);
  protected readonly summary = signal<Array<{ label: string; value: string }>>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly definition = () => definitions[this.area()];

  ngOnInit(): void {
    this.routeSubscription = this.route.paramMap.subscribe(params => {
      const area = params.get('area') as FinancialOperationsArea | null;
      this.area.set(area && definitions[area] ? area : 'electronic-documents');
      this.status.set('');
      this.load();
    });
  }

  ngOnDestroy(): void {
    this.routeSubscription?.unsubscribe();
  }

  protected load(): void {
    if (this.loading()) return;

    this.loading.set(true);
    this.error.set(null);
    const params = this.buildParams();

    this.api.get<unknown>(this.definition().endpoint, params).subscribe({
      next: value => {
        const rows = this.normalizeRows(value);
        this.rows.set(rows);
        this.columns.set(this.resolveColumns(rows));
        this.summary.set(this.resolveSummary(value, rows));
        this.loading.set(false);
      },
      error: error => {
        this.rows.set([]);
        this.columns.set([]);
        this.summary.set([]);
        this.error.set(error instanceof Error ? error.message : 'No fue posible consultar la operación financiera.');
        this.loading.set(false);
      }
    });
  }

  protected cell(row: Record<string, unknown>, key: string): string {
    const value = row[key];
    if (value === null || value === undefined || value === '') return '—';
    if (typeof value === 'boolean') return value ? 'Sí' : 'No';

    if (typeof value === 'number') {
      const normalized = key.toLowerCase();
      if (['amount', 'total', 'subtotal', 'tax', 'credit', 'debit', 'withheld'].some(token => normalized.includes(token))) {
        return new Intl.NumberFormat('es-EC', { style: 'currency', currency: 'USD' }).format(value);
      }
      return new Intl.NumberFormat('es-EC').format(value);
    }

    if (Array.isArray(value)) return String(value.length);
    if (this.isRecord(value)) return JSON.stringify(value);
    return String(value);
  }

  protected label(key: string): string {
    return key
      .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
      .replace(/[_-]+/g, ' ')
      .replace(/^./, value => value.toUpperCase());
  }

  private buildParams(): Record<string, string | number | boolean> {
    const params: Record<string, string | number | boolean> = {};

    if (this.area() === 'electronic-documents') {
      params['page'] = 1;
      params['pageSize'] = 100;
    }

    if (this.definition().periodAware) {
      const range = periodRange(this.period());
      params['startDate'] = range.startDate;
      params['endDate'] = range.endDate;
      params['page'] = 1;
      params['pageSize'] = 100;
    }

    if (this.definition().statusAware && this.status().trim()) {
      params['status'] = this.status().trim();
    }

    if (this.area() === 'tax-legal-review') {
      params['scope'] = 'all';
    }

    return params;
  }

  private normalizeRows(value: unknown): Record<string, unknown>[] {
    if (Array.isArray(value)) return value.filter(this.isRecord);
    if (!this.isRecord(value)) return [];

    for (const key of ['items', 'documents', 'rows', 'results', 'queue', 'actions']) {
      const candidate = value[key];
      if (Array.isArray(candidate)) return candidate.filter(this.isRecord);
    }

    for (const candidate of Object.values(value)) {
      if (Array.isArray(candidate) && candidate.some(this.isRecord)) {
        return candidate.filter(this.isRecord);
      }
    }

    return [];
  }

  private resolveColumns(rows: Record<string, unknown>[]): string[] {
    if (!rows.length) return [];
    const preferred = [
      'documentType', 'documentNumber', 'issueDate', 'customerName', 'supplierName',
      'status', 'environment', 'subtotal', 'taxes', 'taxTotal', 'total', 'totalAmount',
      'accessKeyMasked', 'recommendedNextAction'
    ];
    const keys = Object.keys(rows[0]);
    return [
      ...preferred.filter(key => keys.includes(key)),
      ...keys.filter(key => !preferred.includes(key))
    ].slice(0, 9);
  }

  private resolveSummary(value: unknown, rows: Record<string, unknown>[]): Array<{ label: string; value: string }> {
    const items: Array<{ label: string; value: string }> = [
      { label: 'Registros', value: new Intl.NumberFormat('es-EC').format(rows.length) }
    ];

    if (!this.isRecord(value)) return items;

    for (const [key, current] of Object.entries(value)) {
      if (items.length >= 5) break;
      if (['items', 'documents', 'rows', 'results', 'queue', 'actions'].includes(key)) continue;
      if (typeof current === 'string' || typeof current === 'number' || typeof current === 'boolean') {
        items.push({ label: this.label(key), value: this.scalar(current, key) });
      } else if (this.isRecord(current)) {
        for (const [nestedKey, nestedValue] of Object.entries(current)) {
          if (items.length >= 5) break;
          if (typeof nestedValue === 'string' || typeof nestedValue === 'number' || typeof nestedValue === 'boolean') {
            items.push({ label: this.label(nestedKey), value: this.scalar(nestedValue, nestedKey) });
          }
        }
      }
    }

    return items;
  }

  private scalar(value: string | number | boolean, key: string): string {
    if (typeof value === 'boolean') return value ? 'Sí' : 'No';
    if (typeof value === 'number') {
      const normalized = key.toLowerCase();
      if (['amount', 'total', 'subtotal', 'tax', 'credit', 'debit', 'withheld'].some(token => normalized.includes(token))) {
        return new Intl.NumberFormat('es-EC', { style: 'currency', currency: 'USD' }).format(value);
      }
      return new Intl.NumberFormat('es-EC').format(value);
    }
    return value || '—';
  }

  private readonly isRecord = (value: unknown): value is Record<string, unknown> =>
    typeof value === 'object' && value !== null && !Array.isArray(value);
}

function currentPeriod(): string {
  const now = new Date();
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`;
}

function periodRange(period: string): { startDate: string; endDate: string } {
  const match = /^(\d{4})-(\d{2})$/.exec(period);
  if (!match) {
    const now = new Date();
    const year = now.getFullYear();
    const month = now.getMonth() + 1;
    return {
      startDate: `${year}-${String(month).padStart(2, '0')}-01`,
      endDate: `${year}-${String(month).padStart(2, '0')}-${String(new Date(year, month, 0).getDate()).padStart(2, '0')}`
    };
  }

  const year = Number(match[1]);
  const month = Number(match[2]);
  const lastDay = new Date(year, month, 0).getDate();
  return {
    startDate: `${match[1]}-${match[2]}-01`,
    endDate: `${match[1]}-${match[2]}-${String(lastDay).padStart(2, '0')}`
  };
}
