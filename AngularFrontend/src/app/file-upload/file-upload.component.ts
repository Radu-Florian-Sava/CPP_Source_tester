import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-file-upload',
  templateUrl: './file-upload.component.html',
  styleUrls: ['./file-upload.component.css']
})
export class FileUploadComponent implements OnInit {

  selectedFile: File | any = null;
  response: string;
  outputDisplay: String;
  inputDisplay: String;
  selectInputMessage: String;
  selectOutputMessage: String;
  selectCppMessage: String;
  cppName: String;
  inputName: String;
  outputName: String;
  allFilesLoaded: boolean = false;
  cppDisplay: String;


  constructor(private http: HttpClient) {
    this.response = "No message from server";
    this.cppName = "no Cpp loaded yet";
    this.inputName = "no input loaded yet";
    this.outputName = "no output template loaded yet";
    this.outputDisplay = "block";
    this.cppDisplay = "none";
    this.inputDisplay = "none";
    this.selectCppMessage = "Select cpp source";
    this.selectInputMessage = "Select input file";
    this.selectOutputMessage = "Select output template";
    }

  ngOnInit(): void {
  }

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
    console.log(this.selectedFile);
  }

  onUpload() {
    if (this.selectedFile.name.includes(".cpp")) {
      this.allFilesLoaded = false;
      this.inputDisplay = "block";
      this.cppName = this.selectedFile.name;
    }
    if (this.selectedFile.name.includes("input")) {
      this.inputName = this.selectedFile.name;
      this.allFilesLoaded = true;
    }
      
    if (this.selectedFile.name.includes("output")) {
      this.outputDisplay = "none";
      this.inputDisplay = "block";
      this.cppDisplay = "block";
      this.outputName = this.selectedFile.name;
    }
      
    const fileData = new FormData();
    fileData.append('file', this.selectedFile, this.selectedFile.name);
    this.http.post('/weatherforecast/postFile', fileData)
      .subscribe((res) => {
        var newLabel = document.createElement("label");
        var parentDiv = document.getElementById("serverResponse");
        console.log(res);
        this.response = JSON.stringify(res);
        newLabel.innerHTML = this.response;
        parentDiv?.appendChild(newLabel);
      });
  }
  runCMD() {
    this.outputDisplay = "none";
    var newLabel = document.createElement("label");
    var parentDiv = document.getElementById("serverResponse");
    newLabel.innerHTML = "testing " + this.cppName + " with the following arguments: " + this.inputName + " " + this.outputName;
    parentDiv?.appendChild(newLabel);
    
    const options = {
      headers: new HttpHeaders().append('Content-type', 'application/json')
    }
    this.http.post<string>('/weatherforecast/postRunCMD', JSON.stringify("run"), options)
      .subscribe((res) => {
        var newLabel = document.createElement("label");
        var parentDiv = document.getElementById("serverResponse");
        console.log(res);
        this.response = JSON.stringify(res);
        newLabel.innerHTML = this.response;
        parentDiv?.appendChild(newLabel);
      });
    this.selectCppMessage = "Select new cpp";
    this.selectInputMessage = "Select other input file";
  }

}
