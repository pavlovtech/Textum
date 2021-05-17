import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CreateTextComponent } from './create-text/create-text.component';
import { EditTextComponent } from './edit-text/edit-text.component';
import { TextsComponent } from './texts.component';

const routes: Routes = [
  {
    path: '',
    component: TextsComponent,
    pathMatch: 'full'
  },
  {
    path: 'create',
    component: CreateTextComponent
  },
  {
    path: 'edit/:id',
    component: EditTextComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TextsRoutingModule { }
