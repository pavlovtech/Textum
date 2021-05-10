import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { WordsRoutingModule } from './words-routing.module';
import { WordsComponent } from './words.component';


@NgModule({
  declarations: [
    WordsComponent
  ],
  imports: [
    CommonModule,
    WordsRoutingModule
  ]
})
export class WordsModule { }
