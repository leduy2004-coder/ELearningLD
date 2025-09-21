using Application.Contracts;
using Application.DTOs.Request.AuthGG;
using Application.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleAuthService> _logger;
        private readonly string _googleClientId;

        public GoogleAuthService(HttpClient httpClient, IConfiguration configuration, ILogger<GoogleAuthService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _googleClientId = _configuration["GoogleAuth:ClientId"];
        }

        public async Task<GoogleUserInfo> ValidateGoogleTokenAsync(string idToken)
        {
            try
            {
                _logger.LogInformation("Validating Google ID token");

                var response = await _httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Invalid Google ID token");
                    //throw new AppException(ErrorCode.INVALID_GOOGLE_TOKEN);
                }

                var json = await response.Content.ReadAsStringAsync();
                var tokenInfo = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                // Verify audience (client ID)
                if (!tokenInfo.ContainsKey("aud") || tokenInfo["aud"].ToString() != _googleClientId)
                {
                    _logger.LogError("Google token audience mismatch");
                    throw new AppException(ErrorCode.INVALID_GOOGLE_TOKEN);
                }

                return new GoogleUserInfo
                {
                    Sub = tokenInfo["sub"].ToString(),
                    Email = tokenInfo["email"].ToString(),
                    Name = tokenInfo["name"].ToString(),
                    GivenName = tokenInfo.ContainsKey("given_name") ? tokenInfo["given_name"].ToString() : "",
                    FamilyName = tokenInfo.ContainsKey("family_name") ? tokenInfo["family_name"].ToString() : "",
                    Picture = tokenInfo.ContainsKey("picture") ? tokenInfo["picture"].ToString() : "",
                    EmailVerified = tokenInfo.ContainsKey("email_verified") && bool.Parse(tokenInfo["email_verified"].ToString())
                };
            }
            catch (Exception ex) when (!(ex is AppException))
            {
                _logger.LogError(ex, "Error validating Google token: {Error}", ex.Message);
                throw new AppException(ErrorCode.INVALID_GOOGLE_TOKEN);
            }
        }

        public async Task<GoogleUserInfo> GetGoogleUserInfoAsync(string accessToken)
        {
            try
            {
                _logger.LogInformation("Getting Google user info from access token");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Invalid Google access token");
                    throw new AppException(ErrorCode.INVALID_GOOGLE_TOKEN);
                }

                var json = await response.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return userInfo;
            }
            catch (Exception ex) when (!(ex is AppException))
            {
                _logger.LogError(ex, "Error getting Google user info: {Error}", ex.Message);
                throw new AppException(ErrorCode.INVALID_GOOGLE_TOKEN);
            }
        }
    }
}
