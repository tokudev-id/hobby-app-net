using HobbyApp.Application.Contracts;
using HobbyApp.Application.DTOs;
using HobbyApp.Application.Services.Base.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace HobbyApp.Controllers.Api;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<CreateUserCommandDto> _createUserValidator;
    private readonly IValidator<UpdateUserCommandDto> _updateUserValidator;

    public UsersController(
        IUserService userService,
        IValidator<CreateUserCommandDto> createUserValidator,
        IValidator<UpdateUserCommandDto> updateUserValidator)
    {
        _userService = userService;
        _createUserValidator = createUserValidator;
        _updateUserValidator = updateUserValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? search = null)
    {
        var result = await _userService.GetPagedAsync(page, size, search);

        var meta = new PaginationMeta
        {
            Page = result.Page,
            Size = result.Size,
            Total = result.TotalCount
        };

        var response = ApiResponse<PaginatedResult<UserListItemDto>>.Success(result, meta);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _userService.GetByIdAsync(id);

        if (result == null)
        {
            return NotFound(ApiResponse<UserDetailDto>.Fail("User not found"));
        }

        var response = ApiResponse<UserDetailDto>.Success(result);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommandDto dto)
    {
        // Use FluentValidation
        var validationResult = await _createUserValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
            var response = ApiResponse<object>.Fail($"Validation failed: {errors}");
            return BadRequest(response);
        }

        var result = await _userService.CreateAsync(dto);

        var successResponse = ApiResponse<int>.Success(result);
        return CreatedAtAction(nameof(GetUser), new { id = result }, successResponse);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserCommandDto dto)
    {
        // Authorization: allow only owner or admin
        var isAdmin = User.IsInRole("Admin");
        var userIdClaim = User.FindFirst("userId")?.Value;
        var currentUserId = int.TryParse(userIdClaim, out var parsedId) ? parsedId : 0;
        if (!isAdmin && currentUserId != id)
        {
            return Forbid();
        }

        dto.Id = id;

        // Use FluentValidation
        var validationResult = await _updateUserValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
            var response = ApiResponse<object>.Fail($"Validation failed: {errors}");
            return BadRequest(response);
        }

        await _userService.UpdateAsync(dto);

        var successResponse = ApiResponse<bool>.Success(true);
        return Ok(successResponse);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userService.DeleteAsync(id);

        var response = ApiResponse<bool>.Success(true);
        return Ok(response);
    }
}
