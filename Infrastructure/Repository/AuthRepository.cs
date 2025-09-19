
using Application.Contracts;
using Application.DTOs;
using Application.DTOs.Request;
using Application.DTOs.Response;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Repository
{
    class AuthRepository : IAuth
    {
        private readonly IToken _tokenService;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthRepository> _logger;
        public AuthRepository(IToken tokenService , UserManager<User> userManager, IMapper mapper, ILogger<AuthRepository> logger)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<UserResponse> RegisterUserAsync(UserRegisterRequest request)
        {
            _logger.LogInformation("Registering user");
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogError("Email already exists");
                throw new Exception("Email already exists");
            }

            var newUser = _mapper.Map<User>(request);
            newUser.UserName = request.Email;

            var result = await _userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user: {errors}", errors);
                throw new Exception($"Failed to create user: {errors}");
            }
            _logger.LogInformation("User created successfully");
            var token = await _tokenService.GenerateToken(newUser);
            newUser.CreatedAt = DateTime.Now;
            newUser.UpdatedAt = DateTime.Now;

            var userResponse = _mapper.Map<UserResponse>(newUser);
            userResponse.AccessToken = token;

            return userResponse;
        }
        public async Task<UserResponse> LoginUserAsync(UserLoginRequest request)
        {
            if (request == null)
            {
                _logger.LogError("Login request is null");
                throw new ArgumentNullException(nameof(request));
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                _logger.LogError("Invalid email or password");
                throw new Exception("Invalid email or password");
            }

            // Generate access token
            var token = await _tokenService.GenerateToken(user);

            // Generate refresh token
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Hash the refresh token and store it in the database or override the existing refresh token
            using var sha256 = SHA256.Create();
            var refreshTokenHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));
            user.RefreshToken = Convert.ToBase64String(refreshTokenHash);
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(2);

            user.CreatedAt = DateTime.Now;

            // Update user information in database
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to update user: {errors}", errors);
                throw new Exception($"Failed to update user: {errors}");
            }

            var userResponse = _mapper.Map<User, UserResponse>(user);
            userResponse.AccessToken = token;
            userResponse.RefreshToken = refreshToken;

            return userResponse;
        }
        public async Task<CurrentUserResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            _logger.LogInformation("RefreshToken");

            // Hash the incoming RefreshToken and compare it with the one stored in the database
            using var sha256 = SHA256.Create();
            var refreshTokenHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(request.RefreshToken));
            var hashedRefreshToken = Convert.ToBase64String(refreshTokenHash);

            // Find user based on the refresh token
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == hashedRefreshToken);
            if (user == null)
            {
                _logger.LogError("Invalid refresh token");
                throw new Exception("Invalid refresh token");
            }

            // Validate the refresh token expiry time
            if (user.RefreshTokenExpiryTime < DateTime.Now)
            {
                _logger.LogWarning("Refresh token expired for user ID: {UserId}", user.Id);
                throw new Exception("Refresh token expired");
            }

            // Generate a new access token
            var newAccessToken = await _tokenService.GenerateToken(user);
            _logger.LogInformation("Access token generated successfully");
            var currentUserResponse = _mapper.Map<CurrentUserResponse>(user);
            currentUserResponse.AccessToken = newAccessToken;
            return currentUserResponse;
        }

        public async Task<RevokeRefreshTokenResponse> RevokeRefreshToken(RefreshTokenRequest refreshTokenRemoveRequest)
        {
            _logger.LogInformation("Revoking refresh token");

            try
            {
                // Hash the refresh token
                using var sha256 = SHA256.Create();
                var refreshTokenHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(refreshTokenRemoveRequest.RefreshToken));
                var hashedRefreshToken = Convert.ToBase64String(refreshTokenHash);

                // Find the user based on the refresh token
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == hashedRefreshToken);
                if (user == null)
                {
                    _logger.LogError("Invalid refresh token");
                    throw new Exception("Invalid refresh token");
                }

                // Validate the refresh token expiry time
                if (user.RefreshTokenExpiryTime < DateTime.Now)
                {
                    _logger.LogWarning("Refresh token expired for user ID: {UserId}", user.Id);
                    throw new Exception("Refresh token expired");
                }

                // Remove the refresh token
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;

                // Update user information in database
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to update user");
                    return new RevokeRefreshTokenResponse
                    {
                        Message = "Failed to revoke refresh token"
                    };
                }
                _logger.LogInformation("Refresh token revoked successfully");
                return new RevokeRefreshTokenResponse
                {
                    Message = "Refresh token revoked successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to revoke refresh token: {ex}", ex.Message);
                throw new Exception("Failed to revoke refresh token");
            }
        }


        //public async Task<LoginResponse> LoginUserAsync(LoginDTO loginDTO)
        //{
        //    var getUser = await FindUserByEmail(loginDTO.Email);
        //    if (getUser == null)
        //    {
        //        return new LoginResponse(false, "User not found");
        //    }

        //    bool checkPassword = BCrypt.Net.BCrypt.Verify(loginDTO.Password, getUser.Password);
        //    if (!checkPassword)
        //    {
        //        return new LoginResponse(false, "Invalid password");
        //    }
        //    else
        //    {
        //        return new LoginResponse(true, "Login successful", GenerateJWTToken(getUser));
        //    }
        //}

        //public async Task<RegistrationResponse> RegisterUserAsync(RegisterUserDTO registerUserDTO)
        //{
        //    var getUser = await FindUserByEmail(registerUserDTO.Email!);
        //    if (getUser != null)
        //    {
        //        return new RegistrationResponse(false, "User already exists");
        //    }
        //    appDbContext.Users.Add(new ApplicationUser()
        //    {
        //        UserName = registerUserDTO.UserName,
        //        Email = registerUserDTO.Email,
        //        Password = BCrypt.Net.BCrypt.HashPassword(registerUserDTO.Password)
        //    });
        //    await appDbContext.SaveChangesAsync();
        //    return new RegistrationResponse(true, "User registered successfully");
        //}

        //private String GenerateJWTToken(ApplicationUser user)
        //{
        //    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        //    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        //    var claims = new[]
        //    {
        //        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        //        new Claim(ClaimTypes.Email, user.Email!),
        //        new Claim(ClaimTypes.Name, user.UserName!),
        //        new Claim(ClaimTypes.Role, user.Role!)
        //    };
        //    var token = new JwtSecurityToken(
        //        issuer: configuration["Jwt:Issuer"],
        //        audience: configuration["Jwt:Audience"],
        //        claims: claims,
        //        expires: DateTime.Now.AddMinutes(15),
        //        signingCredentials: credentials
        //    );
        //    return new JwtSecurityTokenHandler().WriteToken(token);

        //}

        //private async Task<ApplicationUser?> FindUserByEmail(string? email)
        //{
        //    return await appDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        //}


    }
}
