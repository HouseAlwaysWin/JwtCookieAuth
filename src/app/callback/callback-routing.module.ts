import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LineCallbackComponent } from './line-callback/line-callback.component';
import { RouterModule, Routes } from '@angular/router';
import { GoogleCallbackComponent } from './google-callback/google-callback.component';


const routes: Routes = [
  { path: 'line', component: LineCallbackComponent },
  { path: 'google', component: GoogleCallbackComponent },
];


@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    RouterModule.forChild(routes)
  ],
  exports: [
    RouterModule
  ]
})
export class CallbackRoutingModule { }
