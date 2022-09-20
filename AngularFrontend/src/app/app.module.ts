import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';

import { FileUploadComponent } from './file-upload/file-upload.component';
import { UserComponent } from './user/user.component';
import { AdminComponent } from './admin/admin.component';
import {RoutingModule} from "./routing/routing.module";

@NgModule({
  declarations: [
    FileUploadComponent,
    UserComponent,
    AdminComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    RoutingModule
  ],
  providers: [],
  bootstrap: [FileUploadComponent]
})
export class AppModule { }
