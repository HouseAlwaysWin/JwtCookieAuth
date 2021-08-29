import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { environment } from 'src/environments/environment';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { LineTokenRes } from './models/lineTokenRes';
import { Guid } from 'guid-typescript';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  env = environment;
  title = 'angular-line-login';
  csrfToken = null;

  lineLoginUrl: string;
  googleLoginUrl: string;

  constructor(
    private http: HttpClient,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.initCsrfToken();
    this.getLineLoginUrl();
    this.getGoogleLoginUrl();

    // this.route.queryParams.subscribe((params: Params) => {
    //   let code = params['code'];
    //   let state = params['state'];

    //   let csrfToken = localStorage.getItem('csrf_token');

    //   if (code && (state === csrfToken)) {
    //     this.http.post(`${this.env.chatBotUrl}api/ThirdPartyAuth/getLineAccessToken`, { code })
    //       .subscribe((result: LineTokenRes) => {
    //         localStorage.setItem('login_token', JSON.stringify(result));
    //         let token = localStorage.getItem('login_token');
    //         window.location.href = '/';
    //         localStorage.removeItem('csrf_token');
    //         console.log(token);
    //       });
    //   }

    // });
  }

  logout() {
    localStorage.removeItem('login_token');
  }

  verifyToken() {
    let token = JSON.parse(localStorage.getItem('login_token'));

    console.log(token?.accessToken);
    if (token?.accessToken) {
      this.http.post(`${this.env.chatBotUrl}api/ThirdPartyAuth/verifyToken`, {
        token: token.accessToken
      })
        .subscribe((result) => {
          console.log(result);
        });
    }
  }


  getProfile() {
    let token = JSON.parse(localStorage.getItem('login_token'));

    console.log(token?.accessToken);
    if (token?.accessToken) {
      this.http.post(`${this.env.chatBotUrl}api/ThirdPartyAuth/getLineProfile`, {
        token: token.accessToken
      })
        .subscribe((result) => {
          console.log(result);
        });
    }
  }

  initCsrfToken() {
    // 避免CSRF跨站攻擊
    let csrfToken = sessionStorage.getItem('csrf_token');
    if (!csrfToken) {
      this.csrfToken = Guid.create();
      sessionStorage.setItem('csrf_token', this.csrfToken.toString());
    }
    else {
      this.csrfToken = csrfToken;
    }
  }

  getLineLoginUrl() {
    this.lineLoginUrl = `${this.env.lineInfo.authUrl}?response_type=code&client_id=${this.env.lineInfo.clientId}&redirect_uri=${this.env.lineInfo.redirectUrl}&state=${this.csrfToken}&scope=profile%20openid%20email`
  }

  getGoogleLoginUrl() {
    this.googleLoginUrl = `${this.env.googleInfo.authUrl}?scope=profile%20openid%20email&include_granted_scopes=true&response_type=token&state=${this.csrfToken}&redirect_uri=${this.env.googleInfo.redirectUrl}&client_id=${this.env.googleInfo.clientId}`;
  }

}
