
using Application.DTOs;
using Application.DTOs.Request;
using Application.DTOs.Response;
using System.Threading.Tasks;

namespace Application.Contracts
{
    public interface IUserService
    {
        Task<CurrentUserResponse> GetCurrentUserAsync();
        Task<UserResponse> GetByIdAsync(Guid id);
        Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request);
        Task DeleteAsync(Guid id);

    }
}
