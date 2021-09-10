import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Guid } from 'guid-typescript';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  env = environment;
  title = 'angular-line-login';
  csrfToken = null;

  lineLoginUrl: string;
  googleLoginUrl: string;

  isLogin: boolean;

  constructor(
    private http: HttpClient,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    // this.initCsrfToken();
    this.getCsrfToken();
    this.isAuth();
  }

  logout() {
    this.http.get(`${this.env.chatBotUrl}api/OAuth/logout`).subscribe(res => {
      window.location.href = '/';
    });
  }

  getCsrfToken() {
    this.http.get<{ token: string }>(`${this.env.chatBotUrl}api/OAuth/getCsrfToken`)
      .subscribe(res => {
        console.log(res);
        localStorage.setItem('csrf_token', res.token.toString());
        this.csrfToken = res.token.toString();
        console.log(this.csrfToken);
        this.getLineLoginUrl();
        this.getGoogleLoginUrl();

      })
  }

  // verifyToken() {
  //   let token = JSON.parse(localStorage.getItem('login_token'));

  //   console.log(token?.accessToken);
  //   if (token?.accessToken) {
  //     this.http.post(`${this.env.chatBotUrl}api/OAuth/verifyToken`, {
  //       token: token.accessToken
  //     })
  //       .subscribe((result) => {
  //         console.log(result);
  //       });
  //   }
  // }


  // getProfile() {
  //   let token = JSON.parse(localStorage.getItem('login_token'));

  //   console.log(token?.accessToken);
  //   if (token?.accessToken) {
  //     this.http.post(`${this.env.chatBotUrl}api/ThirdPartyAuth/getLineProfile`, {
  //       token: token.accessToken
  //     })
  //       .subscribe((result) => {
  //         console.log(result);
  //       });
  //   }
  // }

  // initCsrfToken() {
  //   // 避免CSRF跨站攻擊
  //   let csrfToken = sessionStorage.getItem('csrf_token');
  //   if (!csrfToken) {
  //     this.csrfToken = Guid.create();
  //     sessionStorage.setItem('csrf_token', this.csrfToken.toString());
  //   }
  //   else {
  //     this.csrfToken = csrfToken;
  //   }
  // }

  getLineLoginUrl() {
    this.lineLoginUrl = `${this.env.lineInfo.authUrl}?response_type=code&client_id=${this.env.lineInfo.clientId}&redirect_uri=${this.env.lineInfo.redirectUrl}&state=${this.csrfToken}&scope=profile%20openid%20email`
  }

  getGoogleLoginUrl() {
    this.googleLoginUrl = `${this.env.googleInfo.authUrl}?scope=profile%20openid%20email&include_granted_scopes=true&response_type=code&state=${this.csrfToken}&redirect_uri=${this.env.googleInfo.redirectUrl}&client_id=${this.env.googleInfo.clientId}&prompt=consent`;
  }

  isAuth() {
    this.http.get(`${this.env.chatBotUrl}api/OAuth/isAuth`).subscribe((result: boolean) => {
      this.isLogin = result;
    })
  }


}
