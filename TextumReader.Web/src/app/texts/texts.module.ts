import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TextsRoutingModule } from './texts-routing.module';
import { TextsComponent } from './texts.component';
import { TableModule } from 'primeng/table';
import { TextCreationComponent } from './text-creation/text-creation.component';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button'
import { HttpClientModule } from '@angular/common/http';


@NgModule({
  declarations: [
    TextsComponent,
    TextCreationComponent
  ],
  imports: [
    CommonModule,
    TextsRoutingModule,
    TableModule,
    MatInputModule,
    MatButtonModule,
    HttpClientModule
  ]
})
export class TextsModule { }
