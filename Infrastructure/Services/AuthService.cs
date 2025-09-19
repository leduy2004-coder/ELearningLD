using Application.Contracts;
using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services
{
    public class AuthService : IAuth
    {
        private readonly IToken _tokenService;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IToken tokenService, UserManager<User> userManager, IMapper mapper, ILogger<AuthService> logger)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<UserResponse> RegisterUserAsync(UserRegisterRequest request)
        {
            _logger.LogInformation("Starting user registration process for email: {Email}", request.Email);

            // Kiểm tra user đã tồn tại
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed - email already exists: {Email}", request.Email);
                throw new AppException(ErrorCode.USER_EXISTED);
            }

            // Tạo user mới
            var newUser = _mapper.Map<User>(request);
            newUser.UserName = request.Email;
            newUser.CreatedAt = DateTime.UtcNow;
            newUser.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user {Email}: {Errors}", request.Email, errors);

                // Kiểm tra lỗi password
                if (result.Errors.Any(e => e.Code.Contains("Password")))
                {
                    throw new AppException(ErrorCode.INVALID_PASSWORD);
                }

                throw new AppException(ErrorCode.INVALID_KEY);
            }

            var roles = await _userManager.AddToRoleAsync(newUser, "User");
            // Generate token
            var accessToken = await _tokenService.GenerateToken(newUser);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Lưu refresh token
            await SaveRefreshToken(newUser, refreshToken);

            _logger.LogInformation("User {Email} registered successfully", request.Email);

            var userResponse = _mapper.Map<UserResponse>(newUser);
            userResponse.AccessToken = accessToken;
            userResponse.RefreshToken = refreshToken;

            return userResponse;
        }

        public async Task<UserResponse> LoginUserAsync(UserLoginRequest request)
        {
            _logger.LogInformation("User login attempt for email: {Email}", request.Email);

            if (request == null)
            {
                _logger.LogError("Login request is null");
                throw new AppException(ErrorCode.INVALID_KEY);
            }

            // Tìm user theo email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed - user not found: {Email}", request.Email);
                throw new AppException(ErrorCode.USER_NOT_EXISTED);
            }

            // Kiểm tra password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                _logger.LogWarning("Login failed - incorrect password for user: {Email}", request.Email);
                throw new AppException(ErrorCode.PASSWORD_WRONG);
            }
            var roles = await _userManager.GetRolesAsync(user);
            // Generate tokens
            var accessToken = await _tokenService.GenerateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Lưu refresh token
            await SaveRefreshToken(user, refreshToken);

            // Cập nhật last login
            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("User {Email} logged in successfully", request.Email);

            var userResponse = _mapper.Map<UserResponse>(user);
            userResponse.AccessToken = accessToken;
            userResponse.RefreshToken = refreshToken;

            return userResponse;
        }


        public async Task<CurrentUserResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            _logger.LogInformation("Processing refresh token request");

            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                _logger.LogError("Refresh token is null or empty");
                throw new AppException(ErrorCode.TOKEN_INVALID);
            }

            // Hash refresh token để tìm user
            var hashedRefreshToken = HashRefreshToken(request.RefreshToken);
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == hashedRefreshToken);

            if (user == null)
            {
                _logger.LogWarning("Invalid refresh token provided");
                throw new AppException(ErrorCode.TOKEN_INVALID);
            }

            // Kiểm tra refresh token có expired không
            if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token expired for user: {UserId}", user.Id);

                // Clear expired token
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _userManager.UpdateAsync(user);

                throw new AppException(ErrorCode.RE_TOKEN_EXPIRED);
            }

            // Generate new access token
            var newAccessToken = await _tokenService.GenerateToken(user);

            _logger.LogInformation("Access token refreshed successfully for user: {UserId}", user.Id);

            var currentUserResponse = _mapper.Map<CurrentUserResponse>(user);
            currentUserResponse.AccessToken = newAccessToken;

            return currentUserResponse;
        }


        public async Task<RevokeRefreshTokenResponse> RevokeRefreshToken(RefreshTokenRequest request)
        {
            _logger.LogInformation("Processing refresh token revocation");

            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                _logger.LogError("Refresh token is null or empty");
                throw new AppException(ErrorCode.TOKEN_INVALID);
            }

            // Hash refresh token để tìm user
            var hashedRefreshToken = HashRefreshToken(request.RefreshToken);
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == hashedRefreshToken);

            if (user == null)
            {
                _logger.LogWarning("Invalid refresh token for revocation");
                throw new AppException(ErrorCode.TOKEN_INVALID);
            }

            // Revoke refresh token
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to revoke refresh token for user {UserId}: {Errors}", user.Id, errors);
                throw new AppException(ErrorCode.REVOKE_FAIL);
            }

            _logger.LogInformation("Refresh token revoked successfully for user: {UserId}", user.Id);

            return new RevokeRefreshTokenResponse
            {
                Message = "Refresh token revoked successfully"
            };
        }

        public async Task LogoutUserAsync(string userId)
        {
            _logger.LogInformation("Processing user logout for userId: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found for logout: {UserId}", userId);
                throw new AppException(ErrorCode.USER_NOT_EXISTED);
            }

            // Revoke all refresh tokens
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to logout user {UserId}: {Errors}", userId, errors);
                throw new AppException(ErrorCode.INVALID_KEY);
            }

            _logger.LogInformation("User {UserId} logged out successfully", userId);
        }

        private async Task SaveRefreshToken(User user, string refreshToken)
        {
            var hashedRefreshToken = HashRefreshToken(refreshToken);
            user.RefreshToken = hashedRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // 7 days expiry

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to save refresh token for user {UserId}: {Errors}", user.Id, errors);
                throw new AppException(ErrorCode.INVALID_KEY);
            }
        }

        private static string HashRefreshToken(string refreshToken)
        {
            using var sha256 = SHA256.Create();
            var refreshTokenHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));
            return Convert.ToBase64String(refreshTokenHash);
        }
    }
}
