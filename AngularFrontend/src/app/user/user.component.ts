import { Component, OnInit } from '@angular/core';
import {HttpClient, HttpHeaders} from "@angular/common/http";

@Component({
  selector: 'app-user',
  templateUrl: './user.component.html',
  styleUrls: ['./user.component.css']
})
export class UserComponent implements OnInit {

  selectedFile: File | any = null;
  problems: string[] | null = null ;
  name: string;
  inputExample : string | null = null;
  outputExample : string | null = null;
  description : string | null = null;

  constructor(private _http: HttpClient) {
    this.name = "No source loaded yet";
    this.getProblems();
  }

  ngOnInit(): void {}

  selectSourceFile(event: any): void {
    this.selectedFile = event.target.files[0];
  }

  runCMD() {
    const options = {
      headers: new HttpHeaders().append('Content-type', 'application/json')
    }
    this._http.get<string>('http://localhost:5024/cpptester/getRunCMD', options)
      .subscribe((res) => {
        const newAnswer = document.createElement("li");
        newAnswer.textContent = JSON.stringify(res);
        newAnswer.className += " list-group-item";

        const statusList = document.getElementById("serverResponse");
        statusList?.appendChild(newAnswer);
      });
  }

  getProblems(): void{
    this._http.get('http://localhost:5024/cpptester/getProblems')
      .subscribe((res) => {
        this.problems = res as string[];
      });
  }

  onSubmit() {
    this.name = this.selectedFile.name;
    const fileData = new FormData();
    fileData.append('file', this.selectedFile, this.selectedFile.name);
    this._http.post('http://localhost:5024/cpptester/postSource', fileData)
      .subscribe((res) => {

        const newAnswer = document.createElement("li");
        newAnswer.textContent = JSON.stringify(res);
        newAnswer.className += " list-group-item";

        const statusList = document.getElementById("serverResponse");
        statusList?.appendChild(newAnswer);
      });
  }

  getProblem(problem: string) {
    this._http.get('http://localhost:5024/cpptester/getProblems/' + problem)
      .subscribe((res) => {
        this.description = (res as string[])[0];
        this.inputExample = (res as string[])[1];
        this.outputExample = (res as string[])[2];
      });
  }
}
