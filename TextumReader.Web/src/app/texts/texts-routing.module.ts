import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TextDetailsComponent } from './text-details/text-details.component';
import { TextsComponent } from './texts.component';

const routes: Routes = [
  {
    path: '',
    component: TextsComponent
  },
  {
    path: 'create',
    component: TextDetailsComponent
  },
  {
    path: ':id',
    component: TextDetailsComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TextsRoutingModule { }
