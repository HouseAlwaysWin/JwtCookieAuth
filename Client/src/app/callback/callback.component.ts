// import { HttpClient } from '@angular/common/http';
// import { Component, OnInit } from '@angular/core';
// import { ActivatedRoute, Params, Router } from '@angular/router';
// import { environment } from 'src/environments/environment';
// import { LineTokenRes } from '../models/lineTokenRes';

// @Component({
//   selector: 'app-callback',
//   templateUrl: './callback.component.html',
//   styleUrls: ['./callback.component.scss']
// })
// export class CallbackComponent implements OnInit {
//   env = environment;

//   constructor(
//     private http: HttpClient,
//     private route: ActivatedRoute,
//     private router: Router) { }

//   ngOnInit(): void {
//     console.log('callback');
//     let csrfToken = localStorage.getItem('csrf_token');
//     let state = this.route.snapshot.queryParamMap.get('state');

//     if (state === csrfToken) {
//       let token = this.route.snapshot.queryParamMap.get('access_token');
//       console.log(token);

//       // Line 跳轉
//       if (!token) {
//         this.route.queryParams.subscribe((params: Params) => {
//           let code = params['code'];
//           let state = params['state'];


//           if (code && (state === csrfToken)) {
//             this.http.post(`${this.env.chatBotUrl}api/ThirdPartyAuth/getLineAccessToken`, { code })
//               .subscribe((result: LineTokenRes) => {
//                 console.log(result);
//                 localStorage.setItem('login_token', result.access_token);
//                 let token = localStorage.getItem('login_token');
//                 window.location.href = '/';
//                 localStorage.removeItem('csrf_token');
//                 console.log(token);
//               });
//           }

//         });
//       }
//       else {
//         localStorage.setItem('login_token', token);
//       }

//     }

//     this.router.navigate(['/']);
//   }

// }
