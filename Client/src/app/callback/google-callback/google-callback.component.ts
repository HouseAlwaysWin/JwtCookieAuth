import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { LineTokenRes } from 'src/app/models/lineTokenRes';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-google-callback',
  templateUrl: './google-callback.component.html',
  styleUrls: ['./google-callback.component.scss']
})
export class GoogleCallbackComponent implements OnInit {
  env = environment;
  constructor(
    private http: HttpClient,
    private route: ActivatedRoute) { }

  ngOnInit(): void {
    let code = this.route.snapshot.queryParamMap.get('code');
    let state = this.route.snapshot.queryParamMap.get('state');
    let csrfToken = sessionStorage.getItem('csrf_token');
    console.log(code)
    console.log(state);
    console.log(csrfToken);

    if (code && (state === csrfToken)) {
      this.http.post(`${this.env.chatBotUrl}api/Auth/loginGoogle`, { code })
        .subscribe((result: LineTokenRes) => {
          console.log(result);
          localStorage.setItem('login_token', result.access_token);
          let token = localStorage.getItem('login_token');
          window.location.href = '/';
          localStorage.removeItem('csrf_token');
          console.log(token);
        });
    }


  }

}
