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
import { MatSelectModule } from '@angular/material/select';
import { ViewTextComponent } from './view-text/view-text.component';
import { NgbModule, NgbPopoverModule } from '@ng-bootstrap/ng-bootstrap';

@NgModule({
  declarations: [
    TextsComponent,
    CreateTextComponent,
    EditTextComponent,
    ViewTextComponent
  ],
  imports: [
    CommonModule,
    TextsRoutingModule,
    TableModule,
    MatInputModule,
    MatButtonModule,
    HttpClientModule,
    ReactiveFormsModule,
    MatSelectModule,
    NgbModule
  ]
})
export class TextsModule { }
