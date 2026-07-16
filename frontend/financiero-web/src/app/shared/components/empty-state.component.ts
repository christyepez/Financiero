import { Component, Input } from '@angular/core';

@Component({
  selector: 'fin-empty-state',
  standalone: true,
  template: `
    <div class="panel">
      <h2>{{ title }}</h2>
      <p class="muted">{{ description }}</p>
      <p class="muted">Si esperabas datos, valida el período, los permisos delegados del Portal y que la API local esté disponible.</p>
    </div>
  `
})
export class EmptyStateComponent {
  @Input() title = 'Sin registros';
  @Input() description = 'No hay datos para mostrar en esta vista foundation.';
}
