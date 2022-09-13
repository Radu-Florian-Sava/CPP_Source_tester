import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';

import { FileUploadComponent } from './file-upload/file-upload.component';

@NgModule({
  declarations: [
    FileUploadComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule
  ],
  providers: [],
  bootstrap: [FileUploadComponent]
})
export class AppModule { }
