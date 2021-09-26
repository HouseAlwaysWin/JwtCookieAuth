import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  env = environment;
  isAuth: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  isAuth$ = this.isAuth.asObservable();

  constructor(private http: HttpClient,
    private route: ActivatedRoute) { }


  ngOnInit(): void {

  }

  checkAuth() {
    this.http.get(`${this.env.backendUrl}api/Auth/isAuth`).subscribe((result: boolean) => {
      this.isAuth.next(result);
    });
  }

  logout() {
    this.http.get(`${this.env.backendUrl}api/Auth/logout`).subscribe(res => {
      window.location.href = '/';
    });
  }


}
