import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import { environment } from 'src/environments/environment';
import { AuthRes } from '../models/authRes';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  env = environment;
  authInfo: BehaviorSubject<AuthRes> = new BehaviorSubject<AuthRes>(null);
  authInfo$ = this.authInfo.asObservable();

  constructor(private http: HttpClient,
    private route: ActivatedRoute) { }


  ngOnInit(): void {

  }

  checkAuth() {
    this.http.get(`${this.env.backendUrl}api/Auth/isAuth`).subscribe((result: AuthRes) => {
      this.authInfo.next(result);
    });
  }

  getAntiCrsfToken() {
    return this.http.get(`${this.env.backendUrl}api/Auth/getAntiCsrfToken`).pipe(
      map((res: any) => {
        sessionStorage.setItem('csrf_token', res.token);
      })
    );

  }

  logout() {
    this.http.get(`${this.env.backendUrl}api/Auth/logout`).subscribe(res => {
      window.location.href = '/';
    });
  }


}
