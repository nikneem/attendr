import { Component } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';

@Component({
  selector: 'attn-home-page-component',
  imports: [],
  templateUrl: './home-page-component.html',
  styleUrl: './home-page-component.scss',
})
export class HomePageComponent {
  constructor(private oidcSecurityService: OidcSecurityService) { }

  login() {
    this.oidcSecurityService.authorize();
  }
}
