import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AgentService } from '../../../services/agent.service';
import { Agent } from '../../../models/agent.model';

/** Fiche détaillée d'un agent — GET /api/agents/:id */
@Component({
  selector: 'app-membre-detail',
  imports: [RouterLink],
  templateUrl: './membre-detail.html',
  styleUrl: './membre-detail.css',
})
export class MembreDetailComponent implements OnInit {
  private readonly agentService = inject(AgentService);
  private readonly route        = inject(ActivatedRoute);

  readonly agent   = signal<Agent | null>(null);
  readonly loading = signal(true);
  readonly error   = signal<string | null>(null);

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (isNaN(id)) {
      this.error.set('Identifiant invalide.');
      this.loading.set(false);
      return;
    }
    this.agentService.get(id).subscribe({
      next:  a => { this.agent.set(a); this.loading.set(false); },
      error: () => { this.error.set('Agent introuvable.'); this.loading.set(false); },
    });
  }

  nomComplet(a: Agent): string {
    return [a.nom, a.postnom, a.prenom].filter(Boolean).join(' ');
  }

  formatDate(d?: string): string {
    if (!d) return '—';
    return new Date(d).toLocaleDateString('fr-FR');
  }
}
