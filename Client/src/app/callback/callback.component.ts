import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { environment } from 'src/environments/environment';
import { AuthRes } from '../models/authRes';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-callback',
  template: '',
})
export class CallbackComponent implements OnInit {
  env = environment;

  constructor(
    private authService: AuthService,
    private http: HttpClient,
    private route: ActivatedRoute,
    private router: Router) { }

  ngOnInit(): void {
    let code = this.route.snapshot.queryParamMap.get('code');
    let provider = this.route.snapshot.queryParamMap.get('provider');
    console.log(provider);
    console.log(code)

    if (code) {
      this.authService.getAntiCrsfToken().subscribe(() => {
        this.http.post(`${this.env.backendUrl}api/Auth/ExternalLogin`, {
          Code: code,
          Provider: provider
        }).subscribe((result: AuthRes) => {
          this.authService.authInfo.next(result);
          this.router.navigate(['/']);
        }, error => {
          console.log(error);
          this.router.navigate(['/']);
        });

      })
    }

  }

}
