using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserApiDemo.Application.DTOs;
using UserApiDemo.Application.Interfaces;

namespace UserApiDemo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? name, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        _logger.LogInformation("Get users with filters - Name: {Name}, FromDate: {FromDate}, ToDate: {ToDate}", name, fromDate, toDate);

        if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
        {
            return BadRequest(new ApiResponse { Success = false, Message = "fromDate cannot be greater than toDate" });
        }

        // If no filters provided, fall back to get all
        var users = (string.IsNullOrWhiteSpace(name) && !fromDate.HasValue && !toDate.HasValue)
            ? await _userService.GetAllUsersAsync()
            : await _userService.FilterAsync(name, fromDate, toDate);

        return Ok(new ApiResponse<IEnumerable<UserDto>>
        {
            Success = true,
            Data = users
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("Get user by id: {UserId}", id);
        var user = await _userService.GetUserByIdAsync(id);

        if (user == null)
            return NotFound(new ApiResponse { Success = false, Message = "User not found" });

        return Ok(new ApiResponse<UserDto>
        {
            Success = true,
            Data = user
        });
    }

    [HttpGet("{id}/cached")]
    public async Task<IActionResult> GetByIdCached(Guid id)
    {
        _logger.LogInformation("Get user by id (cached): {UserId}", id);
        try
        {
            var user = await _userService.GetUserByCacheAsync(id);
            return Ok(new ApiResponse<UserDto>
            {
                Success = true,
                Data = user
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse { Success = false, Message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        _logger.LogInformation("Update user: {UserId}", id);

        try
        {
            var user = await _userService.UpdateUserAsync(id, request);
            return Ok(new ApiResponse<UserDto>
            {
                Success = true,
                Message = "User updated successfully",
                Data = user
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse { Success = false, Message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Delete user: {UserId}", id);

        try
        {
            await _userService.DeleteUserAsync(id);
            return Ok(new ApiResponse { Success = true, Message = "User deleted successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse { Success = false, Message = ex.Message });
        }
    }
}
