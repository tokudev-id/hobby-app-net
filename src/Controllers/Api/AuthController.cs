using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using HobbyApp.Application.DTOs;
using HobbyApp.Domain.Entities;
using HobbyApp.Infrastructure.Repositories.Interfaces;
using HobbyApp.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace HobbyApp.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IPasswordHasher<Domain.Entities.User> _passwordHasher;
    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly IValidator<RegisterRequestDto> _registerValidator;

    public AuthController(
        IUnitOfWork unitOfWork, 
        JwtTokenService jwtTokenService,
        IValidator<LoginRequestDto> loginValidator,
        IValidator<RegisterRequestDto> registerValidator)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = new PasswordHasher<Domain.Entities.User>();
        _loginValidator = loginValidator;
        _registerValidator = registerValidator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        // Use FluentValidation
        var validationResult = await _loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage);
            return BadRequest(new { message = "Validation failed", errors = errors });
        }

        var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username);

        if (user == null)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (result != PasswordVerificationResult.Success)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        // Get user roles
        var userRoles = await _unitOfWork.UserRoles.GetUserRolesAsync(user.Id);
        var roleNames = userRoles.Select(r => r.Name);

        var accessToken = _jwtTokenService.GenerateToken(user.Username, user.Email, user.Id, roleNames);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Set HttpOnly cookies
        SetAuthCookies(Response, accessToken, refreshToken);

        return Ok(new
        {
            message = "Login successful",
            username = user.Username,
            fullName = user.FullName,
            email = user.Email,
            roles = roleNames.ToArray()
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            // Use FluentValidation
            var validationResult = await _registerValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage);
                return BadRequest(new { message = "Validation failed", errors = errors });
            }

            // Check if username already exists
            var existingUsername = await _unitOfWork.Users.GetByUsernameAsync(request.Username);
            if (existingUsername != null)
            {
                return BadRequest(new { message = "Username already exists" });
            }

            // Check if email already exists
            var existingEmail = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            if (existingEmail != null)
            {
                return BadRequest(new { message = "Email already exists" });
            }

            // Create new user
            var user = new Domain.Entities.User
            {
                Username = request.Username,
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = _passwordHasher.HashPassword(new Domain.Entities.User(), request.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Hobbies = new List<Domain.Entities.Hobby>()
            };

            // Add hobbies if provided
            if (request.Hobbies != null && request.Hobbies.Any())
            {
                foreach (var hobbyDto in request.Hobbies)
                {
                    var hobby = new Domain.Entities.Hobby
                    {
                        Name = hobbyDto.Name,
                        Level = hobbyDto.Level,
                        CreatedAt = DateTime.UtcNow
                    };
                    user.Hobbies.Add(hobby);
                }
            }

            // Save user
            var createdUser = await _unitOfWork.Users.CreateAsync(user);

            // Assign default User role to new user
            var userRole = await _unitOfWork.Roles.GetByNameAsync("User");
            if (userRole != null)
            {
                await _unitOfWork.UserRoles.CreateAsync(new Domain.Entities.UserRole
                {
                    UserId = createdUser.Id,
                    RoleId = userRole.Id,
                    AssignedAt = DateTime.UtcNow
                });
            }

            // Get user roles for token
            var userRoles = await _unitOfWork.UserRoles.GetUserRolesAsync(createdUser.Id);
            var roleNames = userRoles.Select(r => r.Name);

            // Generate tokens for immediate login
            var accessToken = _jwtTokenService.GenerateToken(user.Username, user.Email, user.Id, roleNames);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            // Set HttpOnly cookies
            SetAuthCookies(Response, accessToken, refreshToken);

            var response = new LoginResponseDto
            {
                Token = accessToken, // Keep for backward compatibility, but client won't use it
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Roles = roleNames.ToArray()
            };

            return CreatedAtAction(nameof(Register), new { id = createdUser.Id }, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while registering the user" });
        }
    }

    private void SetAuthCookies(HttpResponse response, string accessToken, string refreshToken)
    {
        var isProduction = HttpContext.RequestServices
            .GetRequiredService<IConfiguration>()
            .GetValue<bool>("IsProduction");

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction, // HTTPS only in production
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        response.Cookies.Append("auth_token", accessToken, cookieOptions);

        cookieOptions.Expires = DateTime.UtcNow.AddDays(30); // Refresh token lasts longer
        response.Cookies.Append("refresh_token", refreshToken, cookieOptions);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Clear the authentication cookies
        Response.Cookies.Delete("auth_token");
        Response.Cookies.Delete("refresh_token");

        return Ok(new { message = "Logout successful" });
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        // This endpoint will be protected by the JWT middleware
        // If the request reaches here, the user is authenticated
        var userIdClaim = User.FindFirst("userId")?.Value;
        var usernameClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        var rolesClaim = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToArray();

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid user claims" });
        }

        return Ok(new
        {
            user = new
            {
                id = userId,
                username = usernameClaim,
                email = emailClaim,
                roles = rolesClaim
            }
        });
    }
}
