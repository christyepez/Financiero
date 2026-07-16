import { Component } from '@angular/core';

@Component({
  standalone: true,
  selector: 'fin-dashboard',
  template: `
    <section class="grid">
      @for (card of cards; track card.title) {
        <article class="card">
          <h3>{{ card.title }}</h3>
          <p class="muted">{{ card.text }}</p>
        </article>
      }
    </section>
  `
})
export class DashboardComponent {
  protected readonly cards = [
    { title: 'Portal Security', text: 'Sin login propio. Tokens/permisos deben venir del Portal Shell o headers dev controlados.' },
    { title: 'Menú Portal', text: 'El menú financiero se declara como metadata foundation para futura publicación en Portal Menu API.' },
    { title: 'Read-only fiscal', text: 'Las pantallas consultan readiness y catálogos; no envían SRI ni generan documentos oficiales.' },
    { title: 'Sanitización', text: 'Errores y vistas evitan XML completo, claves de acceso, certificados y secretos.' }
  ];
}
