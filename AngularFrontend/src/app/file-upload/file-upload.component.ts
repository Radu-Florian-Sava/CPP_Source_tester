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
  outputDisplay: string;
  inputDisplay: string;
  cppName: string;
  inputName: string;
  outputName: string;
  allFilesLoaded: boolean = false;
  cppDisplay: string;


  constructor(private http: HttpClient) {
    this.response = "No message from server";
    this.cppName = "no Cpp loaded yet";
    this.inputName = "no input loaded yet";
    this.outputName = "no output template loaded yet";
    this.outputDisplay = "block";
    this.cppDisplay = "none";
    this.inputDisplay = "none";
    }

  ngOnInit(): void {

  }

  onFileSelected(event: any): void {
    this.selectedFile = event.target.files[0];
    console.log(this.selectedFile);
  }

  onUpload(): void {
    if (this.selectedFile.name.includes(".cpp")) {
      this.allFilesLoaded = false;
      this.inputDisplay = "block";
      this.cppName = this.selectedFile.name;

      const fileData = new FormData();
      fileData.append('file', this.selectedFile, this.selectedFile.name);
      this.http.post('/cpptester/postSource', fileData)
        .subscribe((res) => {
          const newLabel = document.createElement("label");
          const parentDiv = document.getElementById("serverResponse");
          console.log(res);
          this.response = JSON.stringify(res);
          newLabel.innerHTML = this.response;
          parentDiv?.appendChild(newLabel);
        });
    }
    if (this.selectedFile.name.includes("input")) {
      this.inputName = this.selectedFile.name;
      this.allFilesLoaded = true;

      const fileData = new FormData();
      fileData.append('file', this.selectedFile, this.selectedFile.name);
      this.http.post('/cpptester/postInput', fileData)
        .subscribe((res) => {
          const newLabel = document.createElement("label");
          const parentDiv = document.getElementById("serverResponse");
          console.log(res);
          this.response = JSON.stringify(res);
          newLabel.innerHTML = this.response;
          parentDiv?.appendChild(newLabel);
        });
    }

    if (this.selectedFile.name.includes("output")) {
      this.outputDisplay = "none";
      this.inputDisplay = "block";
      this.cppDisplay = "block";
      this.outputName = this.selectedFile.name;

      const fileData = new FormData();
      fileData.append('file', this.selectedFile, this.selectedFile.name);
      this.http.post('/cpptester/postOutput', fileData)
        .subscribe((res) => {
          const newLabel = document.createElement("label");
          const parentDiv = document.getElementById("serverResponse");
          console.log(res);
          this.response = JSON.stringify(res);
          newLabel.innerHTML = this.response;
          parentDiv?.appendChild(newLabel);
        });
    }
  }
  runCMD() {
    this.outputDisplay = "none";
    const newLabel = document.createElement("label");
    const parentDiv = document.getElementById("serverResponse");
    newLabel.innerHTML = "testing " + this.cppName + " with the following arguments: " + this.inputName + " " + this.outputName;
    parentDiv?.appendChild(newLabel);

    const options = {
      headers: new HttpHeaders().append('Content-type', 'application/json')
    }
    this.http.get<string>('/cpptester/getRunCMD', options)
      .subscribe((res) => {
        const newLabel = document.createElement("label");
        const parentDiv = document.getElementById("serverResponse");
        console.log(res);
        this.response = JSON.stringify(res);
        newLabel.innerHTML = this.response;
        parentDiv?.appendChild(newLabel);
      });
  }

}
