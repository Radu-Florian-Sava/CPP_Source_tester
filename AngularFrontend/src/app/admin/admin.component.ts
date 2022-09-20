import {Component, OnInit} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import * as CryptoJS from 'crypto-js';

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.css']
})

export class AdminComponent implements OnInit {

  inputFile: File | any = null;
  outputFile: File | any = null;
  username: string | null = null;
  password: string | null = null;
  token: string | null = null;

  constructor(private _http: HttpClient) {
  }

  ngOnInit(): void {
  }

  onSubmit() {
    this.checkCredentials();
    if (this.token !== null) {
      this.submitInputFile();
      this.submitOutputFile();
    }
  }

  private checkCredentials() {
    if (this.password === null || this.username === null || this.token !== null) {
      return;
    }
    let hashedPassword = CryptoJS.SHA256(this.password).toString();
    this._http.post('http://localhost:5024/cpptester/postCredentials',
      {
        "username": this.username,
        "password": hashedPassword
      }).subscribe((res) => {
      console.log(res);
      this.token = res as string;
    });
  }

  private submitInputFile() {
    const fileData = new FormData();
    fileData.append('file', this.inputFile, this.token as string);
    this._http.post('http://localhost:5024/cpptester/postInput', fileData)
      .subscribe((res) => {
        const newAnswer = document.createElement("li");
        newAnswer.innerText = JSON.stringify(res);
        newAnswer.className += " list-group-item";

        const statusList = document.getElementById("serverResponse");
        statusList?.appendChild(newAnswer);
      });
  }

  private submitOutputFile() {
    const fileData = new FormData();
    fileData.append('file', this.outputFile, this.token as string);
    this._http.post('http://localhost:5024/cpptester/postOutput', fileData)
      .subscribe((res) => {
        const newAnswer = document.createElement("li");
        newAnswer.innerText = JSON.stringify(res);
        newAnswer.className += " list-group-item";

        const statusList = document.getElementById("serverResponse");
        statusList?.appendChild(newAnswer);
      });
  }

  selectInputFile(event: any) {
    this.inputFile = event.target.files[0];
  }

  selectOutputFile(event: any) {
    this.outputFile = event.target.files[0];
  }

}
