
using Application.Contracts;
using Application.DTOs;
using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services
{
    class UserService : IUserService
    {

        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(UserManager<User> userManager, IMapper mapper, ILogger<UserService> logger)
        {
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<UserResponse> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting user by id");
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                _logger.LogError("User not found");
                throw new AppException(ErrorCode.USER_NOT_EXISTED);
            }
            _logger.LogInformation("User found");
            return _mapper.Map<UserResponse>(user);
        }
        public async Task<CurrentUserResponse> GetCurrentUserAsync()
        {
            var user = await _userManager.FindByIdAsync(GetInfo.GetUserId());
            if (user == null)
            {
                _logger.LogError("User not found");
                throw new AppException(ErrorCode.USER_NOT_EXISTED);
            }
            return _mapper.Map<CurrentUserResponse>(user);
        }

        public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                _logger.LogError("User not found");
                throw new AppException(ErrorCode.USER_NOT_EXISTED);
            }

            user.UpdatedAt = DateTime.Now;
            user.FullName = request.FullName;
            user.Email = request.Email;
            user.Gender = request.Gender;

            await _userManager.UpdateAsync(user);
            return _mapper.Map<UserResponse>(user);
        }
        public async Task DeleteAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                _logger.LogError("User not found");
                throw new AppException(ErrorCode.USER_NOT_EXISTED);
            }
            await _userManager.DeleteAsync(user);
        }
    }

}
