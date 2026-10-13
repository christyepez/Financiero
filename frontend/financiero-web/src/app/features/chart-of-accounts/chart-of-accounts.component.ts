import { Component, inject, signal } from '@angular/core';
import { AccountingApiService } from '../../core/services/accounting-api.service';
import { CreateFinancialAccount, FinancialAccount } from '../../core/services/api.models';
import { ErrorMessageComponent } from '../../shared/components/error-message.component';
import { LoadingStateComponent } from '../../shared/components/loading-state.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';

@Component({
  standalone: true,
  selector: 'fin-chart-of-accounts',
  imports: [ErrorMessageComponent, LoadingStateComponent, StatusBadgeComponent],
  template: `
    <section class="panel stack">
      <div class="section-title">
        <div><h2>Plan de cuentas</h2><p class="muted">Catálogo contable operativo gobernado por Portal Security.</p></div>
        <span class="badge info">{{ accounts().length }} cuentas</span>
      </div>
      <div class="form-grid">
        <input aria-label="Buscar cuentas" type="search" [value]="search()" (input)="search.set($any($event.target).value)" placeholder="Buscar código o nombre">
        <button type="button" (click)="load()">Buscar</button>
      </div>
      <fin-loading-state [loading]="loading()" />
      <fin-error-message [message]="error()" />
      <div class="table-scroll">
        <table class="table">
          <thead><tr><th>Código</th><th>Nombre</th><th>Tipo</th><th>Nivel</th><th>Movimiento</th><th>Estado</th></tr></thead>
          <tbody>
            @for (account of accounts(); track account.id) {
              <tr>
                <td><strong>{{ account.code }}</strong></td><td>{{ account.name }}</td><td>{{ account.type }}</td>
                <td>{{ account.level }}</td><td>{{ account.isMovementAccount ? 'Sí' : 'No' }}</td>
                <td><fin-status-badge [value]="account.status" /></td>
              </tr>
            } @empty { <tr><td colspan="6">No existen cuentas con los filtros actuales.</td></tr> }
          </tbody>
        </table>
      </div>
    </section>

    <section class="panel stack">
      <h2>Nueva cuenta</h2>
      <p class="muted">Alta real mediante <code>financial.chartofaccounts.create</code>.</p>
      <div class="form-grid">
        <input type="text" [value]="code()" (input)="code.set($any($event.target).value)" placeholder="Código">
        <input type="text" [value]="name()" (input)="name.set($any($event.target).value)" placeholder="Nombre">
        <select [value]="type()" (change)="type.set($any($event.target).value)">
          @for (value of accountTypes; track value) { <option [value]="value">{{ value }}</option> }
        </select>
        <input type="number" min="1" [value]="level()" (input)="level.set(+$any($event.target).value)" placeholder="Nivel">
        <label class="checkbox-field"><input type="checkbox" [checked]="movement()" (change)="movement.set($any($event.target).checked)"> Cuenta de movimiento</label>
        <button type="button" [disabled]="saving()" (click)="create()">{{ saving() ? 'Guardando…' : 'Crear cuenta' }}</button>
      </div>
      @if (success()) { <div class="notice ok">{{ success() }}</div> }
    </section>
  `
})
export class ChartOfAccountsComponent {
  private readonly api = inject(AccountingApiService);
  protected readonly accounts = signal<FinancialAccount[]>([]);
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);
  protected readonly search = signal('');
  protected readonly code = signal('');
  protected readonly name = signal('');
  protected readonly type = signal('Asset');
  protected readonly level = signal(1);
  protected readonly movement = signal(true);
  protected readonly accountTypes = ['Asset','Liability','Equity','Income','Expense','Cost','Memo','Other'];

  constructor() { this.load(); }

  protected load(): void {
    this.loading.set(true); this.error.set(null);
    this.api.getAccounts(this.search().trim()).subscribe({
      next: response => this.accounts.set(response.items ?? []),
      error: error => { this.error.set(error.message); this.loading.set(false); },
      complete: () => this.loading.set(false)
    });
  }

  protected create(): void {
    const request: CreateFinancialAccount = {
      code: this.code().trim(), name: this.name().trim(), type: this.type(),
      level: Math.max(1, this.level()), parentAccountId: null, isMovementAccount: this.movement()
    };
    if (!request.code || !request.name) { this.error.set('Código y nombre son obligatorios.'); return; }
    this.saving.set(true); this.error.set(null); this.success.set(null);
    this.api.createAccount(request).subscribe({
      next: created => { this.success.set(`Cuenta ${created.code} creada correctamente.`); this.code.set(''); this.name.set(''); this.load(); },
      error: error => { this.error.set(error.message); this.saving.set(false); },
      complete: () => this.saving.set(false)
    });
  }
}
