using UserApiDemo.Application.DTOs;
using UserApiDemo.Application.Interfaces;
using UserApiDemo.Domain.Entities;
using UserApiDemo.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace UserApiDemo.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IDistributedCache _cache;
    private const string UserCacheKeyPrefix = "user_";
    private const int CacheExpirationMinutes = 30;

    public UserService(IUserRepository userRepository, IDistributedCache cache)
    {
        _userRepository = userRepository;
        _cache = cache;
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user == null ? null : MapToUserDto(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToUserDto).ToList();
    }

    public async Task<IEnumerable<UserDto>> FilterAsync(string? name, DateTime? fromDate, DateTime? toDate)
    {
        var users = await _userRepository.FilterAsync(name, fromDate, toDate);
        return users.Select(MapToUserDto).ToList();
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"User with id {id} not found");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.DateOfBirth = request.DateOfBirth;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        await InvalidateUserCacheAsync(id);

        return MapToUserDto(user);
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"User with id {id} not found");

        await _userRepository.DeleteAsync(user);
        await _userRepository.SaveChangesAsync();
        await InvalidateUserCacheAsync(id);
    }

    public async Task<UserDto> GetUserByCacheAsync(Guid id)
    {
        var cacheKey = $"{UserCacheKeyPrefix}{id}";

        // Try to get from cache
        var cachedUser = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedUser))
        {
            return JsonSerializer.Deserialize<UserDto>(cachedUser)
                ?? throw new InvalidOperationException("Failed to deserialize cached user");
        }

        // Get from database
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"User with id {id} not found");

        var userDto = MapToUserDto(user);

        // Cache the result
        var options = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheExpirationMinutes));

        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(userDto), options);

        return userDto;
    }

    public async Task InvalidateUserCacheAsync(Guid id)
    {
        var cacheKey = $"{UserCacheKeyPrefix}{id}";
        await _cache.RemoveAsync(cacheKey);
    }

    private UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DateOfBirth = user.DateOfBirth,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
