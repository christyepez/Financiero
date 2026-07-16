import { Component } from '@angular/core';

@Component({
  selector: 'fin-portal-context-required',
  standalone: true,
  template: `
    <section class="panel">
      <span class="badge bad">Portal context required</span>
      <h2>Financiero requiere PortalCorporativo en producción</h2>
      <p class="muted">No se crea login propio ni fallback standalone productivo. Solicita al Portal Shell inyectar contexto de usuario, tenant, permisos, menú, flags y correlación.</p>
    </section>
  `
})
export class PortalContextRequiredComponent {}
