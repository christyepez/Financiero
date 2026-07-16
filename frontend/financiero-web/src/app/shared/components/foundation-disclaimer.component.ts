import { Component, Input } from '@angular/core';

@Component({
  selector: 'fin-foundation-disclaimer',
  standalone: true,
  template: `
    <div class="warning">
      <span class="badge warn">Foundation / No productivo</span>
      {{ text || 'Foundation / No productivo. No habilita SRI producción, ATS oficial, RIDE legal final ni aprobaciones mutables.' }}
    </div>
  `
})
export class FoundationDisclaimerComponent {
  @Input() text = '';
}
