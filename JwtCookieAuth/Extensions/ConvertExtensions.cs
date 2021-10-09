using JwtCookieAuth.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JwtCookieAuth.Extensions
{
    public static class ConvertExtensions
    {
        /// <summary>
        /// 字串轉整數,失敗回傳整數預設值0
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int ConvertIntOrDefault(this string s)
        {
            int result = 0;
            int.TryParse(s, out result);
            return result;
        }

        public static List<Claim> ConvertToClaims(this Dictionary<string, string> dic)
        {
            List<Claim> claims = new List<Claim>();
            foreach (var claimDic in dic)
            {
                claims.Add(new Claim(claimDic.Key, claimDic.Value));
            }
            return claims;
        }

        public static Dictionary<string, string> ConvertToDictionary(this List<Claim> claims)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (var claim in claims)
            {
                dic.Add(claim.Type, claim.Value);
            }
            return dic;
        }

        public static List<Claim> ConvertToClaims(this OAuthUserInfoRes userInfo)
        {
            var claims = new List<Claim>{
                new Claim(JwtRegisteredClaimNames.UniqueName,userInfo.Name),
                new Claim("Provider",userInfo.Provider.ToString()),
            };
            if (!string.IsNullOrEmpty(userInfo.Email))
            {
                claims.Add(new Claim("Email", userInfo.Email));
            }

            if (!string.IsNullOrEmpty(userInfo.PictureUrl))
            {
                claims.Add(new Claim("PictureUrl", userInfo.PictureUrl));
            }

            if (!string.IsNullOrEmpty(userInfo.Id))
            {
                claims.Add(new Claim("Id", userInfo.Id));
            }

            return claims;
        }
    }

}
