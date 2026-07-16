import { Component, Input } from '@angular/core';

@Component({
  selector: 'fin-empty-state',
  standalone: true,
  template: `
    <div class="panel">
      <h2>{{ title }}</h2>
      <p class="muted">{{ description }}</p>
    </div>
  `
})
export class EmptyStateComponent {
  @Input() title = 'Sin registros';
  @Input() description = 'No hay datos para mostrar en esta vista foundation.';
}
