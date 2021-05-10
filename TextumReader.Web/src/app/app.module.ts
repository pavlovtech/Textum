import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AuthModule } from '@auth0/auth0-angular';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AuthButtonComponent } from './auth-button/auth-button.component';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { AuthHttpInterceptor } from '@auth0/auth0-angular';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
@NgModule({
  declarations: [
    AppComponent,
    AuthButtonComponent
  ],
  imports: [
    BrowserAnimationsModule,
    BrowserModule,
    AppRoutingModule,
    AuthModule.forRoot({
      domain: 'textum.eu.auth0.com',
      clientId: 'sZwMqs8JGRHVv8x9s9Hn2eRntoxW99dr',
      audience: 'https://textumreader.com/api/translator',
      scope: 'read:translations',
      httpInterceptor: {
        allowedList: [
          {
            uri: 'https://localhost:5050/*',
            tokenOptions: {
              audience: 'https://textumreader.com/api/translator',
              scope: 'read:translations'
            }
          }
        ]
      }
    }),
    HttpClientModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AuthHttpInterceptor, multi: true },
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
