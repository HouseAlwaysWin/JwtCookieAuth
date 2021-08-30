import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
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
    private router: Router,
    private route: ActivatedRoute) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe((params: Params) => {
      let code = params['code'];
      let state = params['state'];
      let csrfToken = localStorage.getItem('csrf_token');
      console.log(state);
      console.log(csrfToken);
      console.log(code);

      if (code && (state === csrfToken)) {
        this.http.post(`${this.env.chatBotUrl}api/Account/getGoogleAccessToken`, { code })
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
