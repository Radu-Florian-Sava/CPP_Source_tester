import {Component, OnInit} from '@angular/core';
import {HttpClient, HttpErrorResponse} from "@angular/common/http";
import * as CryptoJS from 'crypto-js';

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.css']
})

export class AdminComponent implements OnInit {

  descriptionFile: File| any;
  inputFile: File| any;
  outputFile: File| any;
  username: string | null = null;
  password: string | null = null;
  token: string | null = null;

  constructor(private _http: HttpClient) {}

  ngOnInit(): void {}

  async onSubmit() {
    await this.checkCredentials();
    if (this.token !== null) {

      this.submitFile(this.descriptionFile, 'http://localhost:5024/cpptester/postDescription');
      this.submitFile(this.inputFile, 'http://localhost:5024/cpptester/postInput');
      this.submitFile(this.outputFile, 'http://localhost:5024/cpptester/postOutput');
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

  public submitFile(file: File| any, url: string): void {
    const fileData = new FormData();
    fileData.append('file', file, this.token as string);
    this._http.post(url, fileData)
      .subscribe((res) => {
        const newAnswer = document.createElement("li");
        newAnswer.innerText = JSON.stringify(res);
        newAnswer.className += " list-group-item";

        const statusList = document.getElementById("serverResponse");
        statusList?.appendChild(newAnswer);
      });
  }

  public selectDescriptionFile(event: any): void{
    this.descriptionFile = event.target.files[0];
  }

  public selectInputFile(event: any): void{
    this.inputFile = event.target.files[0];
  }

  public selectOutputFile(event: any): void{
    this.outputFile = event.target.files[0];
  }

}
