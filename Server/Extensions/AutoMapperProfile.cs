using AutoMapper;
using JwtCookieAuth.Models;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Extensions
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<OAuthUserInfoRes, ExternalAuthResponse>();
        }
    }
}
