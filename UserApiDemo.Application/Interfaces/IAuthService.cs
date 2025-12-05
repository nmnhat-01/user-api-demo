using UserApiDemo.Application.DTOs;
using UserApiDemo.Domain.Entities;

namespace UserApiDemo.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    string GenerateToken(User user);
}
