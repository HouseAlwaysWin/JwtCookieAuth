using Server.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace Server.Services
{
    public interface ITokenService
    {
        string CreateJwtToken(string secretKey, string issuer, List<Claim> claims);
        RefreshToken GenerateRefreshToken(string ipAddress);

    }
}