using UserApiDemo.Application.DTOs;
using UserApiDemo.Domain.Entities;

namespace UserApiDemo.Application.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<IEnumerable<UserDto>> FilterAsync(string? name, DateTime? fromDate, DateTime? toDate);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task DeleteUserAsync(Guid id);
    Task<UserDto> GetUserByCacheAsync(Guid id);
    Task InvalidateUserCacheAsync(Guid id);
}
