using Application.Contracts;
using Application.DTOs.Request;
using Application.DTOs.Request.AuthGG;
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
        private readonly IEmailService _emailService;
        private readonly IGoogleAuthService _googleAuthService;
        public AuthService(IToken tokenService,IEmailService emailService,IGoogleAuthService googleAuthService, UserManager<User> userManager, IMapper mapper, ILogger<AuthService> logger)
        {
            _tokenService = tokenService;
            _emailService = emailService;
            _googleAuthService = googleAuthService;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task RegisterUserAsync(UserRegisterRequest request)
        {
            _logger.LogInformation("Starting user registration process for email: {Email}", request.Email);

            // Check user exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed - email already exists: {Email}", request.Email);
                throw new AppException(ErrorCode.USER_EXISTED);
            }

            // Create new user
            var newUser = _mapper.Map<User>(request);
            newUser.UserName = request.FullName;
            newUser.CreatedAt = DateTime.UtcNow;
            newUser.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user {Email}: {Errors}", request.Email, errors);

                // Check error password
                if (result.Errors.Any(e => e.Code.Contains("Password")))
                {
                    throw new AppException(ErrorCode.INVALID_PASSWORD);
                }

                throw new AppException(ErrorCode.INVALID_USERNAME);
            }

            var roles = await _userManager.AddToRoleAsync(newUser, "User");
            if (!roles.Succeeded)
            {
                _logger.LogWarning("Failed to add role to user {Email}", request.Email);
            }

            try
            {
                // Send EMAIL VERIFICATION 
                var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                await _emailService.SendEmailVerificationAsync(request.Email, confirmationToken);

                _logger.LogInformation("Email verification sent to {Email}", request.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email to {Email}: {Error}", request.Email, ex.Message);
            }
        }

        public async Task<UserResponse> LoginUserAsync(UserLoginRequest request)
        {
            _logger.LogInformation("User login attempt for email: {Email}", request.Email);

            if (request == null)
            {
                _logger.LogError("Login request is null");
                throw new AppException(ErrorCode.UNCATEGORIZED_EXCEPTION);
            }

            // Search user for email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed - user not found: {Email}", request.Email);
                throw new AppException(ErrorCode.USER_NOT_EXISTED);
            }

            // Check password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                _logger.LogWarning("Login failed - incorrect password for user: {Email}", request.Email);
                throw new AppException(ErrorCode.PASSWORD_WRONG);
            }
            var roles = await _userManager.GetRolesAsync(user);

            // 3. Check email verified
            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("Login failed - email not verified: {Email}", request.Email);
                throw new AppException(ErrorCode.EMAIL_NOT_VERIFIED);
            }

            // Generate tokens
            var accessToken = await _tokenService.GenerateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Save refresh token
            await SaveRefreshToken(user, refreshToken);

            // Update last login
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

            // Hash refresh token for search user
            var hashedRefreshToken = HashRefreshToken(request.RefreshToken);
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == hashedRefreshToken);

            if (user == null)
            {
                _logger.LogWarning("Invalid refresh token provided");
                throw new AppException(ErrorCode.TOKEN_INVALID);
            }

            // Check refresh token has expired 
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

            // Hash refresh token for search user
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
                throw new AppException(ErrorCode.UNCATEGORIZED_EXCEPTION);
            }

            _logger.LogInformation("User {UserId} logged out successfully", userId);
        }

        public async Task<UserResponse> GoogleLoginAsync(GoogleAuthRequest request)
        {
            _logger.LogInformation("Processing Google login");

            GoogleUserInfo googleUser;

            // Validate Google token and get user info
            if (!string.IsNullOrEmpty(request.IdToken))
            {
                googleUser = await _googleAuthService.ValidateGoogleTokenAsync(request.IdToken);
            }
            else if (!string.IsNullOrEmpty(request.AccessToken))
            {
                googleUser = await _googleAuthService.GetGoogleUserInfoAsync(request.AccessToken);
            }
            else
            {
                _logger.LogError("No Google token provided");
                throw new AppException(ErrorCode.UNCATEGORIZED_EXCEPTION);
            }

            if (string.IsNullOrEmpty(googleUser.Email))
            {
                _logger.LogError("Google user email is empty");
                throw new AppException(ErrorCode.INVALID_GOOGLE_TOKEN);
            }

            _logger.LogInformation("Google authentication successful for email: {Email}", googleUser.Email);

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(googleUser.Email);

            if (existingUser != null)
            {
                // Existing user - login process
                return await HandleExistingGoogleUser(existingUser, googleUser);
            }
            else
            {
                // New user - registration process
                return await HandleNewGoogleUser(googleUser);
            }
        }

        private async Task<UserResponse> HandleExistingGoogleUser(User user, GoogleUserInfo googleUser)
        {
            _logger.LogInformation("Handling existing Google user: {Email}", user.Email);

            // Update user info from Google if needed
            bool needUpdate = false;

            if (string.IsNullOrEmpty(user.FullName) && !string.IsNullOrEmpty(googleUser.GivenName))
            {
                user.FullName = googleUser.GivenName;
                needUpdate = true;
            }

            //if (string.IsNullOrEmpty(user.Avatar) && !string.IsNullOrEmpty(googleUser.Picture))
            //{
            //    user.Avatar = googleUser.Picture;
            //    needUpdate = true;
            //}

            // Mark email as verified since it's verified by Google
            if (!user.EmailConfirmed && googleUser.EmailVerified)
            {
                user.EmailConfirmed = true;
                needUpdate = true;

                _logger.LogInformation("Email automatically verified through Google for user: {Email}", user.Email);
            }

            if (needUpdate)
            {
                user.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }

            // Generate tokens
            var accessToken = await _tokenService.GenerateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            await SaveRefreshToken(user, refreshToken);

            _logger.LogInformation("Existing Google user logged in successfully: {Email}", user.Email);

            var userResponse = _mapper.Map<UserResponse>(user);
            userResponse.AccessToken = accessToken;
            userResponse.RefreshToken = refreshToken;

            return userResponse;
        }

        private async Task<UserResponse> HandleNewGoogleUser(GoogleUserInfo googleUser)
        {
            _logger.LogInformation("Creating new user from Google account: {Email}", googleUser.Email);

            var newUser = new User
            {
                UserName = googleUser.Email,
                Email = googleUser.Email,
                FullName = googleUser.GivenName,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LockoutEnabled = false
            };

            var result = await _userManager.CreateAsync(newUser);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create Google user {Email}: {Errors}", googleUser.Email, errors);
                throw new AppException(ErrorCode.UNCATEGORIZED_EXCEPTION);
            }

            await _userManager.AddToRoleAsync(newUser, "User");

            var accessToken = await _tokenService.GenerateToken(newUser);
            var refreshToken = _tokenService.GenerateRefreshToken();
            await SaveRefreshToken(newUser, refreshToken);

            // Send email welcome
            try
            {
                await _emailService.SendWelcomeEmailAsync(newUser.Email, newUser.FullName ?? newUser.UserName);
                _logger.LogInformation("Welcome email sent for new Google user: {Email}", newUser.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email for Google user: {Email}", newUser.Email);
            }

            _logger.LogInformation("New Google user created successfully: {Email}", googleUser.Email);

            var userResponse = _mapper.Map<UserResponse>(newUser);
            userResponse.AccessToken = accessToken;
            userResponse.RefreshToken = refreshToken;

            return userResponse;
        }
        public async Task VerifyEmailAsync(VerifyEmailRequest request)
        {
            _logger.LogInformation("Processing email verification for email: {Email}", request.Email);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Email verification attempt for non-existent email: {Email}", request.Email);
                throw new AppException(ErrorCode.USER_NOT_EXISTED);
            }
            if (user.EmailConfirmed)
            {
                _logger.LogInformation("Email already verified for user: {Email}", request.Email);
                return;
            }
            // Confirm email using token
            var decodedToken = Uri.UnescapeDataString(request.VerificationToken);
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to verify email for user {Email}: {Errors}", request.Email, errors);
                throw new AppException(ErrorCode.TOKEN_INVALID);
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            try
            {
                await _emailService.SendWelcomeEmailAsync(user.Email, user.UserName ?? user.Email);
                _logger.LogInformation("Welcome email sent to {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}: {Error}", user.Email, ex.Message);
               
            }

            _logger.LogInformation("Email verified successfully for user: {Email}", request.Email);
        }

        public async Task ResendVerificationAsync(ResendVerificationRequest request)
        {
            _logger.LogInformation("Processing resend verification for email: {Email}", request.Email);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Không throw exception để tránh user enumeration attack
                _logger.LogWarning("Resend verification request for non-existent email: {Email}", request.Email);
                return;
            }

            if (user.EmailConfirmed)
            {
                _logger.LogInformation("Email already verified for user: {Email}", request.Email);
                return;
            }

            // Generate email confirmation token
            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // TODO: Send verification email
            await _emailService.SendEmailVerificationAsync(user.Email, confirmationToken);

            _logger.LogInformation("Verification email resent for user: {Email}", request.Email);
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
                throw new AppException(ErrorCode.UNCATEGORIZED_EXCEPTION);
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
