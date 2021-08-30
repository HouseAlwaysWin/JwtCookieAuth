import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { LineTokenRes } from 'src/app/models/lineTokenRes';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-line-callback',
  templateUrl: './line-callback.component.html',
  styleUrls: ['./line-callback.component.scss']
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
      let csrfToken = localStorage.getItem('csrf_token');
      console.log(state);
      console.log(csrfToken);
      console.log(code);

      if (code && (state === csrfToken)) {
        this.http.post(`${this.env.chatBotUrl}api/Account/getLineAccessToken`, { code })
          .subscribe((result: LineTokenRes) => {
            localStorage.setItem('login_token', result.access_token);
            let token = localStorage.getItem('login_token');
            localStorage.removeItem('csrf_token');
            this.router.navigate(['/']);
          });
      }

    });

  }

}
