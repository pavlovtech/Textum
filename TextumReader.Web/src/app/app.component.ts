import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  constructor(private http: HttpClient) {
    http.post('https://localhost:5050/api/translator/word-translation', {
      from: 'en',
      to: 'ru',
      text: 'dude'
    }).subscribe(data => {
      console.log(data);
    });
    
  }
}
