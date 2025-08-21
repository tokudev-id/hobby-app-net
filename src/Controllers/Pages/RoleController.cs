using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HobbyApp.Application.Services.Base.User;
using HobbyApp.Infrastructure.Repositories.Interfaces;

namespace HobbyApp.Controllers.Pages;

[Route("roles")]
[Authorize(Roles = "Admin")]
public class RoleController : Controller
{
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RoleController> _logger;

    public RoleController(IUserService userService, IUnitOfWork unitOfWork, ILogger<RoleController> logger)
    {
        _userService = userService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        try
        {
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading role management page");
            return View("Error");
        }
    }

    [HttpGet("assign/{userId}")]
    public async Task<IActionResult> AssignRole(int userId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.UserId = userId;
            ViewBag.UserName = user.Username;
            ViewBag.UserEmail = user.Email;
            
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading assign role page for user {UserId}", userId);
            return View("Error");
        }
    }
}
