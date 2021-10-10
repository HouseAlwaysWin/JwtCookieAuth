import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { LineTokenRes } from 'src/app/models/lineTokenRes';
import { AuthService } from 'src/app/services/auth.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-line-callback',
  template: '',
})
export class LineCallbackComponent implements OnInit {
  env = environment;

  constructor(private http: HttpClient,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe((params: Params) => {
      let code = params['code'];
      let state = params['state'];
      let csrfToken = localStorage.getItem('csrf_token');

      if (code) {
        const options = {
          headers: new HttpHeaders({ "X-XSRF-TOKEN": csrfToken }),
        };
        console.log(code);
        this.http.post(`${this.env.backendUrl}api/Auth/ExternalLogin`, {
          Code: code,
          Provider: 'Line'
        }, options)
          .subscribe((result: any) => {
            this.authService.isAuth.next(true);
            this.router.navigate(['/']);
          });
      }

    });

  }

}
