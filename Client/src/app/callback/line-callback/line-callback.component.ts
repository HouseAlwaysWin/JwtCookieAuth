import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { LineTokenRes } from 'src/app/models/lineTokenRes';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-line-callback',
  template: '',
})
export class LineCallbackComponent implements OnInit {
  env = environment;

  constructor(private http: HttpClient,
    private route: ActivatedRoute,
    private router: Router) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe((params: Params) => {
      let code = params['code'];
      let state = params['state'];
      let csrfToken = sessionStorage.getItem('csrf_token');

      if (code && (state === csrfToken)) {
        this.http.post(`${this.env.chatBotUrl}api/OAuth/Login`, {
          code: code,
          Provider: 'Line'
        })
          .subscribe((result: any) => {
            localStorage.setItem('token', result.token);
            let token = localStorage.getItem('token');
            console.log(token);
            sessionStorage.removeItem('csrf_token');
            this.router.navigate(['/']);
          });
      }

    });

  }

}
