import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import { ErrorMessageComponent } from '../../shared/components/error-message.component';
import { LoadingStateComponent } from '../../shared/components/loading-state.component';

type WorkspaceArea = 'accounts' | 'journal-entries' | 'fiscal-years' | 'fiscal-periods';

interface WorkspaceDefinition {
  title: string;
  description: string;
  endpoint: string;
  columns: Array<{ key: string; label: string }>;
}

const definitions: Record<WorkspaceArea, WorkspaceDefinition> = {
  accounts: {
    title: 'Plan de cuentas',
    description: 'Consulta y administra el catálogo contable gobernado por el Portal.',
    endpoint: '/api/financial/accounts',
    columns: [
      { key: 'code', label: 'Código' },
      { key: 'name', label: 'Cuenta' },
      { key: 'type', label: 'Tipo' },
      { key: 'level', label: 'Nivel' },
      { key: 'isMovementAccount', label: 'Movimiento' },
      { key: 'status', label: 'Estado' }
    ]
  },
  'journal-entries': {
    title: 'Asientos contables',
    description: 'Consulta asientos, débitos/créditos y ejecuta el posteo de borradores.',
    endpoint: '/api/financial/journal-entries',
    columns: [
      { key: 'entryNumber', label: 'Número' },
      { key: 'postingDate', label: 'Fecha' },
      { key: 'reference', label: 'Referencia' },
      { key: 'description', label: 'Descripción' },
      { key: 'totalDebit', label: 'Debe' },
      { key: 'totalCredit', label: 'Haber' },
      { key: 'status', label: 'Estado' }
    ]
  },
  'fiscal-years': {
    title: 'Años fiscales',
    description: 'Administra la vigencia de los ejercicios fiscales.',
    endpoint: '/api/financial/fiscal-years',
    columns: [
      { key: 'year', label: 'Año' },
      { key: 'startDate', label: 'Inicio' },
      { key: 'endDate', label: 'Fin' },
      { key: 'status', label: 'Estado' }
    ]
  },
  'fiscal-periods': {
    title: 'Períodos fiscales',
    description: 'Consulta, abre, cierra, bloquea y reabre períodos fiscales.',
    endpoint: '/api/financial/fiscal-periods',
    columns: [
      { key: 'periodNumber', label: 'Período' },
      { key: 'startDate', label: 'Inicio' },
      { key: 'endDate', label: 'Fin' },
      { key: 'status', label: 'Estado' }
    ]
  }
};

@Component({
  standalone: true,
  selector: 'fin-accounting-workspace',
  imports: [ErrorMessageComponent, LoadingStateComponent],
  template: `
    <section class="panel">
      <div class="workspace-heading">
        <div>
          <p class="eyebrow">Contabilidad</p>
          <h2>{{ definition().title }}</h2>
          <p class="muted">{{ definition().description }}</p>
        </div>
        <button type="button" (click)="load()" [disabled]="loading()">Actualizar</button>
      </div>

      <div class="accounting-summary">
        <article><strong>{{ rows().length }}</strong><span>Registros</span></article>
        <article><strong>{{ activeCount() }}</strong><span>Activos / abiertos</span></article>
        <article><strong>{{ draftCount() }}</strong><span>Borradores</span></article>
      </div>

      <div class="period-selector">
        <label>Buscar
          <input type="search"
                 [value]="search()"
                 (input)="search.set($any($event.target).value)"
                 (keyup.enter)="load()"
                 placeholder="Código, nombre, referencia">
        </label>
        <button type="button" (click)="load()" [disabled]="loading()">Consultar</button>
      </div>
    </section>

    <fin-loading-state [loading]="loading()" />
    <fin-error-message [message]="error()" />

    @if (!loading()) {
      <section class="panel accounting-grid-panel">
        <div class="table-scroll">
          <table class="table accounting-grid">
            <thead>
              <tr>
                @for (column of definition().columns; track column.key) {
                  <th>{{ column.label }}</th>
                }
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              @for (row of rows(); track row['id'] ?? $index) {
                <tr>
                  @for (column of definition().columns; track column.key) {
                    <td>
                      @if (column.key === 'status') {
                        <span class="badge info">{{ cell(row, column.key) }}</span>
                      } @else {
                        {{ cell(row, column.key) }}
                      }
                    </td>
                  }
                  <td class="row-actions">
                    @for (action of actionsFor(row); track action.action) {
                      <button type="button"
                              class="action-button"
                              [disabled]="actionBusy() === actionKey(row, action.action)"
                              (click)="runAction(row, action.action)">
                        {{ action.label }}
                      </button>
                    }
                  </td>
                </tr>
              } @empty {
                <tr>
                  <td [attr.colspan]="definition().columns.length + 1">
                    No hay registros para los filtros actuales.
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </section>
    }
  `
})
export class AccountingWorkspaceComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(ApiService);
  private routeSubscription?: Subscription;

  protected readonly area = signal<WorkspaceArea>('accounts');
  protected readonly rows = signal<Record<string, unknown>[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly search = signal('');
  protected readonly actionBusy = signal<string | null>(null);

  protected readonly definition = () => definitions[this.area()];

  ngOnInit(): void {
    this.routeSubscription = this.route.paramMap.subscribe(params => {
      const area = params.get('area') as WorkspaceArea | null;
      this.area.set(area && definitions[area] ? area : 'accounts');
      this.search.set('');
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

    const params: Record<string, string | number | boolean> = { page: 1, pageSize: 100 };
    const search = this.search().trim();
    if (search && (this.area() === 'accounts' || this.area() === 'journal-entries')) {
      params['search'] = search;
    }

    this.api.get<unknown>(this.definition().endpoint, params).subscribe({
      next: value => {
        this.rows.set(this.normalizeRows(value));
        this.loading.set(false);
      },
      error: error => {
        this.error.set(error instanceof Error ? error.message : 'No fue posible consultar el módulo contable.');
        this.loading.set(false);
      }
    });
  }

  protected activeCount(): number {
    return this.rows().filter(row => {
      const status = String(row['status'] ?? '').toLowerCase();
      return ['active', 'open', 'posted'].includes(status);
    }).length;
  }

  protected draftCount(): number {
    return this.rows().filter(row => String(row['status'] ?? '').toLowerCase() === 'draft').length;
  }

  protected cell(row: Record<string, unknown>, key: string): string {
    const value = row[key];
    if (value === null || value === undefined || value === '') return '—';
    if (typeof value === 'boolean') return value ? 'Sí' : 'No';
    if (typeof value === 'number') {
      if (key.toLowerCase().includes('debit') || key.toLowerCase().includes('credit')) {
        return new Intl.NumberFormat('es-EC', { style: 'currency', currency: 'USD' }).format(value);
      }
      return new Intl.NumberFormat('es-EC').format(value);
    }
    return String(value);
  }

  protected actionsFor(row: Record<string, unknown>): Array<{ action: string; label: string }> {
    const status = String(row['status'] ?? '').toLowerCase();
    switch (this.area()) {
      case 'accounts':
        if (status === 'active') return [{ action: 'deactivate', label: 'Desactivar' }, { action: 'archive', label: 'Archivar' }];
        if (status === 'inactive') return [{ action: 'activate', label: 'Activar' }, { action: 'archive', label: 'Archivar' }];
        return [];
      case 'journal-entries':
        return status === 'draft' ? [{ action: 'post', label: 'Postear' }] : [];
      case 'fiscal-years':
        if (status === 'draft' || status === 'closed') return [{ action: 'open', label: 'Abrir' }, { action: 'archive', label: 'Archivar' }];
        if (status === 'open') return [{ action: 'close', label: 'Cerrar' }];
        return [];
      case 'fiscal-periods':
        if (status === 'draft' || status === 'closed') return [{ action: 'open', label: 'Abrir' }, { action: 'archive', label: 'Archivar' }];
        if (status === 'open') return [{ action: 'close', label: 'Cerrar' }, { action: 'lock', label: 'Bloquear' }];
        if (status === 'locked') return [{ action: 'reopen', label: 'Reabrir' }];
        return [];
    }
  }

  protected actionKey(row: Record<string, unknown>, action: string): string {
    return `${String(row['id'] ?? '')}:${action}`;
  }

  protected runAction(row: Record<string, unknown>, action: string): void {
    const id = String(row['id'] ?? '');
    if (!id || this.actionBusy()) return;

    const key = this.actionKey(row, action);
    this.actionBusy.set(key);
    this.error.set(null);
    this.api.post<unknown>(`${this.definition().endpoint}/${encodeURIComponent(id)}/${action}`, {}).subscribe({
      next: () => {
        this.actionBusy.set(null);
        this.load();
      },
      error: error => {
        this.actionBusy.set(null);
        this.error.set(error instanceof Error ? error.message : 'No fue posible ejecutar la acción.');
      }
    });
  }

  private normalizeRows(value: unknown): Record<string, unknown>[] {
    if (Array.isArray(value)) return value.filter(this.isRecord);
    if (!this.isRecord(value)) return [];

    for (const candidate of [value['items'], value['data'], value['results']]) {
      if (Array.isArray(candidate)) return candidate.filter(this.isRecord);
    }

    return [];
  }

  private readonly isRecord = (value: unknown): value is Record<string, unknown> =>
    typeof value === 'object' && value !== null && !Array.isArray(value);
}
