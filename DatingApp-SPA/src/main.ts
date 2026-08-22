import { enableProdMode, importProvidersFrom, provideZoneChangeDetection } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';


import { environment } from './environments/environment';
import { AuthService } from './app/_services/auth.service';
import { ErrorInterceptorProvider } from './app/_services/error.interceptor';
import { AlertifyService } from './app/_services/alertify.service';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { BrowserModule, bootstrapApplication } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { provideAnimations } from '@angular/platform-browser/animations';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { provideRouter } from '@angular/router';
import { appRoutes } from './app/app/routes';
import { AppComponent } from './app/app/app.component';

if (environment.production) {
  enableProdMode();
}

bootstrapApplication(AppComponent, {
    providers: [
        provideZoneChangeDetection(),importProvidersFrom(BrowserModule, FormsModule, BsDropdownModule.forRoot()),
        AuthService,
        ErrorInterceptorProvider,
        AlertifyService,
        provideHttpClient(withInterceptorsFromDi()),
        provideAnimations(),
        provideRouter(appRoutes)
    ]
})
  .catch(err => console.error(err));

