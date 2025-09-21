using Application.DTOs.Request.AuthGG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts
{
    public interface IGoogleAuthService
    {
        Task<GoogleUserInfo> ValidateGoogleTokenAsync(string idToken);
        Task<GoogleUserInfo> GetGoogleUserInfoAsync(string accessToken);
    }
}

