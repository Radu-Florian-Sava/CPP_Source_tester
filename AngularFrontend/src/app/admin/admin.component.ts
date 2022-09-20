import {Component, OnInit} from '@angular/core';
import {HttpClient} from "@angular/common/http";

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.css']
})

export class AdminComponent implements OnInit {

  inputFile : File | any = null;
  outputFile : File | any = null;

  constructor(private _http: HttpClient) { }

  ngOnInit(): void {
  }

  onSubmit() {
    this.submitInputFile();
    this.submitOutputFile();
  }

  private submitInputFile(){
      const fileData = new FormData();
      fileData.append('file', this.inputFile, this.inputFile.name);
      this._http.post('http://localhost:5024/cpptester/postInput', fileData)
        .subscribe((res) => {
          const newAnswer = document.createElement("li");
          newAnswer.innerText = JSON.stringify(res);
          newAnswer.className += " list-group-item";

          const statusList = document.getElementById("serverResponse");
          statusList?.appendChild(newAnswer);
        });
  }

  private submitOutputFile(){
      const fileData = new FormData();
      fileData.append('file', this.outputFile, this.outputFile.name);
      this._http.post('http://localhost:5024/cpptester/postOutput', fileData)
        .subscribe((res) => {
          const newAnswer = document.createElement("li");
          newAnswer.innerText = JSON.stringify(res);
          newAnswer.className += " list-group-item";

          const statusList = document.getElementById("serverResponse");
          statusList?.appendChild(newAnswer);
        });
  }

  selectInputFile(event: any){
    this.inputFile = event.target.files[0];
  }

  selectOutputFile(event: any){
    this.outputFile = event.target.files[0];
  }
}
