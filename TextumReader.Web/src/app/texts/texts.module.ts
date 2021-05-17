import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TextsRoutingModule } from './texts-routing.module';
import { TextsComponent } from './texts.component';
import { TableModule } from 'primeng/table';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button'
import { HttpClientModule } from '@angular/common/http';
import { ReactiveFormsModule } from '@angular/forms';
import { CreateTextComponent } from './create-text/create-text.component';
import { EditTextComponent } from './edit-text/edit-text.component';


@NgModule({
  declarations: [
    TextsComponent,
    CreateTextComponent,
    EditTextComponent
  ],
  imports: [
    CommonModule,
    TextsRoutingModule,
    TableModule,
    MatInputModule,
    MatButtonModule,
    HttpClientModule,
    ReactiveFormsModule
  ]
})
export class TextsModule { }
