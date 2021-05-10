import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-texts',
  templateUrl: './texts.component.html',
  styleUrls: ['./texts.component.scss']
})
export class TextsComponent implements OnInit {

  texts = [
    {
      name: 'Text 1',
      description: 'Text description'
    }
  ];

  constructor() { }

  ngOnInit(): void {
  }
}
