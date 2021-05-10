import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TextCreationComponent } from './text-creation/text-creation.component';
import { TextsComponent } from './texts.component';

const routes: Routes = [
  {
    path: '',
    component: TextsComponent
  },
  {
    path: 'create',
    component: TextCreationComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TextsRoutingModule { }
