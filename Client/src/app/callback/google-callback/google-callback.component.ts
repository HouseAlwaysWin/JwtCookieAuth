import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { LineTokenRes } from 'src/app/models/lineTokenRes';
import { AuthService } from 'src/app/services/auth.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-google-callback',
  template: '',
})
export class GoogleCallbackComponent implements OnInit {
  env = environment;
  constructor(
    private authService: AuthService,
    private http: HttpClient,
    private route: ActivatedRoute) { }

  ngOnInit(): void {
    let code = this.route.snapshot.queryParamMap.get('code');
    let state = this.route.snapshot.queryParamMap.get('state');
    let csrfToken = localStorage.getItem('csrf_token');
    console.log(code)
    console.log(state);
    console.log(csrfToken);

    if (code) {
      const options = {
        headers: new HttpHeaders({ "X-XSRF-TOKEN": csrfToken })
      };

      this.http.post(`${this.env.backendUrl}api/Auth/ExternalLogin`, {
        Code: code,
        Provider: 'Google'
      }, options).subscribe((result: any) => {
        this.authService.isAuth.next(true);
        window.location.href = '/';
        // localStorage.removeItem('csrf_token');
      }, error => {
        window.location.href = '/';
      });
    }


  }

}
