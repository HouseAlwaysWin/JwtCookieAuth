using Microsoft.AspNetCore.Authentication.OAuth;
using Server.Models;

namespace Server.OauthPrividers
{
	public interface IOAuthProvider
	{
	   string CreateLoginUrl();
		AuthResponse GetAuthResponseInfo(OAuthTokenResponse res);
	}
}