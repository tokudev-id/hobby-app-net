using System.Security.Claims;
using HobbyApp.Application.DTOs;
using HobbyApp.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace HobbyApp.Controllers.Api;

[ApiController]
[Route("api/roles")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RolesController> _logger;
    private readonly IValidator<AssignRoleDto> _assignRoleValidator;

    public RolesController(
        IUnitOfWork unitOfWork, 
        ILogger<RolesController> logger,
        IValidator<AssignRoleDto> assignRoleValidator)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _assignRoleValidator = assignRoleValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        try
        {
            var roles = await _unitOfWork.Roles.GetAllAsync();
            var roleDtos = roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description
            }).ToList();

            return Ok(roleDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles");
            return StatusCode(500, new { message = "An error occurred while retrieving roles" });
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserRoles(int userId)
    {
        try
        {
            // Check if current user is admin or accessing their own data
            var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && currentUserId != userId)
            {
                return Forbid("You can only view your own roles unless you are an admin");
            }

            var userRoles = await _unitOfWork.UserRoles.GetUserRolesAsync(userId);
            var roleDtos = userRoles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description
            }).ToList();

            return Ok(roleDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user roles for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while retrieving user roles" });
        }
    }

    [HttpPost("assign")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto request)
    {
        try
        {
            // Use FluentValidation
            var validationResult = await _assignRoleValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
                return BadRequest(new { message = $"Validation failed: {errors}" });
            }

            // Check if user exists
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Check if role exists
            var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            // Check if user already has this role
            var existingUserRoles = await _unitOfWork.UserRoles.GetUserRolesAsync(request.UserId);
            if (existingUserRoles.Any(r => r.Id == request.RoleId))
            {
                return BadRequest(new { message = "User already has this role" });
            }

            // Assign role
            var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            var userRole = new Domain.Entities.UserRole
            {
                UserId = request.UserId,
                RoleId = request.RoleId,
                AssignedBy = currentUserId,
                AssignedAt = DateTime.UtcNow
            };

            await _unitOfWork.UserRoles.AddAsync(userRole);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Role assigned successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user {UserId}", request.UserId);
            return StatusCode(500, new { message = "An error occurred while assigning role" });
        }
    }

    [HttpDelete("unassign")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UnassignRole([FromBody] AssignRoleDto request)
    {
        try
        {
            // Use FluentValidation
            var validationResult = await _assignRoleValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
                return BadRequest(new { message = $"Validation failed: {errors}" });
            }

            // Find the user role assignment
            var userRoles = await _unitOfWork.UserRoles.GetAllAsync();
            var userRole = userRoles.FirstOrDefault(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId);

            if (userRole == null)
            {
                return NotFound(new { message = "Role assignment not found" });
            }

            // Don't allow removing the last Admin role if this is the only admin
            var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId);
            if (role?.Name == "Admin")
            {
                var adminRoleAssignments = userRoles.Where(ur => ur.RoleId == request.RoleId).Count();
                if (adminRoleAssignments <= 1)
                {
                    return BadRequest(new { message = "Cannot remove the last admin role" });
                }
            }

            await _unitOfWork.UserRoles.DeleteAsync(userRole.Id);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Role unassigned successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unassigning role from user {UserId}", request.UserId);
            return StatusCode(500, new { message = "An error occurred while unassigning role" });
        }
    }
}
