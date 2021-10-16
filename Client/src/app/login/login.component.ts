import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { environment } from 'src/environments/environment';
import { UserInfo } from '../models/authRes';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  env = environment;
  title = 'angular-line-login';

  lineLoginUrl: string = '';
  googleLoginUrl: string = '';

  isLogin: boolean = false;
  userInfo: UserInfo = null;


  constructor(
    private http: HttpClient,
    public authService: AuthService
  ) { }

  ngOnInit(): void {
    this.authService.authInfo$.subscribe(res => {
      if (res) {
        this.isLogin = res.isAuth;
        this.userInfo = res.userInfo;
      }
    });

    this.getLineLoginUrl();
    this.getGoogleLoginUrl();
  }

  logout() {
    this.authService.logout();
  }


  getLineLoginUrl() {
    this.http.get(`${this.env.backendUrl}api/auth/getOAuthLoginUrl?provider=Line`).subscribe((res: any) => {
      this.lineLoginUrl = res.url;
    })
  }

  getGoogleLoginUrl() {
    this.http.get(`${this.env.backendUrl}api/auth/getOAuthLoginUrl?provider=Google`).subscribe((res: any) => {
      this.googleLoginUrl = res.url;
    })
  }



}
