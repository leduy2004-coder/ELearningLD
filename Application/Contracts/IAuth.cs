
using Application.DTOs;
using Application.DTOs.Request;
using Application.DTOs.Response;
using System.Threading.Tasks;

namespace Application.Contracts
{
    public interface IAuth
    {
        Task<UserResponse> RegisterUserAsync(UserRegisterRequest registerUserDTO);
        Task<UserResponse> LoginUserAsync(UserLoginRequest loginDTO);
        Task<RevokeRefreshTokenResponse> RevokeRefreshToken(RefreshTokenRequest refreshTokenRemoveRequest);
        Task<CurrentUserResponse> RefreshTokenAsync(RefreshTokenRequest request);
    }
}
