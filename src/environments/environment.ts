// This file can be replaced during build by using the `fileReplacements` array.
// `ng build` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
  production: false,
  chatBotUrl: 'https://localhost:44394/',
  lineInfo: {
    authUrl: 'https://access.line.me/oauth2/v2.1/authorize',
    redirectUrl: 'http://localhost:4200/callback/line',
    clientId: '1656348599'
  },
  googleInfo: {
    authUrl: 'https://accounts.google.com/o/oauth2/v2/auth',
    redirectUrl: 'http://localhost:4200/callback/google',
    clientId: '563822801624-brvnu1sftmi78lfntvkk9s38jc4ubke7.apps.googleusercontent.com'
  }
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
// import 'zone.js/plugins/zone-error';  // Included with Angular CLI.
