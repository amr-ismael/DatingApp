import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard  {
  private authService = inject(AuthService);
  private router = inject(Router);
  private alertify = inject(AlertifyService);

  canActivate(): boolean {
    if (this.authService.loggedIn()) {
      return true;
    }
    this.alertify.error('You shall not pass !');
    this.router.navigate(['']);
    return false;
  }
}
