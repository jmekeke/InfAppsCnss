import { Component, inject, signal, computed } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet, Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AgentService } from '../../services/agent.service';
import { Agent } from '../../models/agent.model';

/**
 * MembresComponent — page principale Membres.
 *
 * Sous-navigation (tabs) :
 *   /membres/liste      → liste complète des agents RH
 *   /membres/recherche  → recherche par nom et/ou entité
 *   /membres/:id        → fiche détaillée d'un agent
 */
@Component({
  selector: 'app-membres',
  imports: [RouterLink, RouterLinkActive, RouterOutlet, FormsModule],
  templateUrl: './membres.html',
  styleUrl: './membres.css',
})
export class MembresComponent {}
