import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GoogleCallbackComponent } from './google-callback/google-callback.component';
import { LineCallbackComponent } from './line-callback/line-callback.component';
import { CallbackRoutingModule } from './callback-routing.module';



@NgModule({
  declarations: [
    GoogleCallbackComponent,
    LineCallbackComponent
  ],
  imports: [
    CommonModule,
    CallbackRoutingModule
  ]
})
export class CallbackModule { }
