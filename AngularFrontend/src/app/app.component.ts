import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  public forecasts?: WeatherForecast[];

  constructor(private http: HttpClient) {
    http.get<WeatherForecast[]>('/weatherforecast/get').subscribe(result => {
      this.forecasts = result;
    }, error => console.error(error));
  }

  onSendClick(text: {sentText:string}) {
    console.log(text.sentText);
    const options = {
      headers: new HttpHeaders().append('Content-type', 'application/json')
    }
    this.http.post<string>('/weatherforecast/postText',JSON.stringify(text.sentText),options)
      .subscribe((res) => {
        console.log(res);
      });
  }

  title = 'AngularFrontend';
}

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}
