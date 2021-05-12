import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: 'texts',
    loadChildren: () => import('./texts/texts.module').then(m => m.TextsModule)
  },
  {
    path: 'words',
    loadChildren: () => import('./words/words.module').then(m => m.WordsModule)
  },
  {
    path: '',
    redirectTo: '/texts', 
    pathMatch: 'full' 
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
