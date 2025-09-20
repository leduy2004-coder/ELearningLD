using Application.Contracts;
using Application.DTOs;
using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuth _auth;

        public AuthController(IAuth userService)
        {
            _auth = userService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
        {
            await _auth.RegisterUserAsync(request);
            return Ok(ApiResponse.Success(true));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            var response = await _auth.LoginUserAsync(request);
            return Ok(ApiResponse.Success(response));
        }

        [HttpPost("refresh-token")]
        [Authorize]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var response = await _auth.RefreshTokenAsync(request);
            return Ok(ApiResponse.Success(response));
        }

        [HttpPost("revoke-refresh-token")]
        [Authorize]
        public async Task<IActionResult> RevokeRefreshToken([FromBody] RefreshTokenRequest request)
        {
            var response = await _auth.RevokeRefreshToken(request);
            if (response != null && response.Message == "Refresh token revoked successfully")
            {
                return Ok(ApiResponse.Success(response));
            }
            throw new AppException(ErrorCode.REVOKE_FAIL);
        }

        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            await _auth.VerifyEmailAsync(request);
            return Ok(ApiResponse.Success(true));
        }

        [HttpPost("resend-verification")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
        {
            await _auth.ResendVerificationAsync(request);
            return Ok(ApiResponse.Success(true));
        }
    }
}
