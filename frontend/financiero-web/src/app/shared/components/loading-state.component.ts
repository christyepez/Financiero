import { Component, Input } from '@angular/core';

@Component({
  selector: 'fin-loading-state',
  standalone: true,
  template: `@if (loading) { <div class="panel muted">Cargando datos sanitizados del backend...</div> }`
})
export class LoadingStateComponent {
  @Input() loading = false;
}
