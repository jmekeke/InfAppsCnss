import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

/**
 * Page Forbidden — affichée par roleGuard quand l'utilisateur est authentifié
 * mais ne possède pas le rôle requis pour la route demandée.
 */
@Component({
  selector: 'app-forbidden',
  imports: [RouterLink],
  templateUrl: './forbidden.html',
})
export class ForbiddenComponent {}
