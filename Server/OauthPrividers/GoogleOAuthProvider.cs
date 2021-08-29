using System.Net;
using System.Web;
using System;
using Server.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;

namespace Server.OauthPrividers
{
    public class GoogleOAuthProvider : IOAuthProvider
    {
        private readonly string ACCESS_TOKEN = "access_token";

        private readonly string CLIENT_ID = "client_id";

        private readonly string CLIENT_SECRET = "client_secret";

        private readonly string CODE = "code";

        private readonly string REDIRECT_URI = "redirect_uri";

        private readonly string RESPONSE_TYPE = "response_type";

        private readonly string SCOPE = "scope";

        private readonly string PROMPT = "prompt";

        private readonly string STATE = "state";

        private readonly string GRANT_TYPE = "grant_type";

        private readonly string _chooseAccountUrl = "https://accounts.google.com/o/oauth2/v2/auth/oauthchooseaccount";

        private readonly string AUTHORIZE_URL = "https://accounts.google.com/o/oauth2/v2/auth";

        private readonly string ACCESSTOKEN_URL = "https://www.googleapis.com/oauth2/v4/token";

        private readonly string USERINFO_URL = "https://www.googleapis.com/oauth2/v3/userinfo";
		private readonly OAuthProviderConfig _config;

		public GoogleOAuthProvider(OAuthProviderConfig config)
        {
			this._config = config;
		}

        public string CreateLoginUrl(){
            var url = QueryHelpers.AddQueryString(this.AUTHORIZE_URL,
            new Dictionary<string,string>{
                { this.CLIENT_ID, this._config.AppId},
                { this.RESPONSE_TYPE, this.CODE},
                { this.REDIRECT_URI, this._config.RedirectUrl},
                { this.SCOPE, this._config.Scope},
                { this.STATE, Guid.NewGuid().ToString()}
            });
            return  url;
        }
    }
}