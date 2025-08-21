using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HobbyApp.Application.Services.Base.User;
using HobbyApp.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;
using HobbyApp.Infrastructure.Repositories.Interfaces;
using FluentValidation;

namespace HobbyApp.Controllers.Pages
{
    [Route("")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateUserCommandDto> _createUserValidator;
        private readonly IValidator<UpdateUserCommandDto> _updateUserValidator;

        public UserController(
            IUserService userService, 
            ILogger<UserController> logger, 
            IUnitOfWork unitOfWork,
            IValidator<CreateUserCommandDto> createUserValidator,
            IValidator<UpdateUserCommandDto> updateUserValidator)
        {
            _userService = userService;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _createUserValidator = createUserValidator;
            _updateUserValidator = updateUserValidator;
        }

        [Authorize]
        [HttpGet("/users")]
        public async Task<IActionResult> Index(int page = 1, int size = 10, string search = "")
        {
            try
            {
                if (User.Identity?.IsAuthenticated == false)
                {
                    // User is not logged in, redirect to login page
                    return RedirectToAction("Login", "Account");
                }
                var result = await _userService.GetPagedAsync(page, size, search);

                ViewBag.CurrentPage = page;
                ViewBag.PageSize = size;
                ViewBag.Search = search;
                ViewBag.TotalPages = (int)Math.Ceiling((double)result.TotalCount / size);

                return View(result.Items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                TempData["ErrorMessage"] = "Error loading users. Please try again.";
                return View(new List<UserListItemDto>());
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("/users/create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                // Get available roles for selection
                var roles = await _unitOfWork.Roles.GetAllAsync();
                ViewBag.AvailableRoles = roles.Select(r => new SelectListItem 
                { 
                    Value = r.Id.ToString(), 
                    Text = r.Name 
                }).ToList();

                var model = new CreateUserCommandDto();
                // Default to "User" role
                var userRole = roles.FirstOrDefault(r => r.Name == "User");
                if (userRole != null)
                {
                    model.RoleIds.Add(userRole.Id);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create user form");
                TempData["ErrorMessage"] = "Error loading form. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("/users/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserCommandDto model)
        {
            // Use FluentValidation
            var validationResult = await _createUserValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                // Add FluentValidation errors to ModelState
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = await _userService.CreateAsync(model);
                    TempData["SuccessMessage"] = "User created successfully!";
                    _logger.LogInformation("User {Id} created by {User}", userId, User.Identity?.Name);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating user");
                    ModelState.AddModelError("", "Error creating user. Please try again.");
                }
            }

            // Repopulate roles for the view in case of validation errors
            try
            {
                var roles = await _unitOfWork.Roles.GetAllAsync();
                ViewBag.AvailableRoles = roles.Select(r => new SelectListItem 
                { 
                    Value = r.Id.ToString(), 
                    Text = r.Name 
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading roles for form");
            }

            return View(model);
        }

        [Authorize]
        [HttpGet("/users/edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Authorization: allow only owner or admin
                var isAdmin = User.IsInRole("Admin");
                var userIdClaim = User.FindFirst("userId")?.Value;
                var currentUserId = int.TryParse(userIdClaim, out var parsedId) ? parsedId : 0;
                if (!isAdmin && currentUserId != id)
                {
                    return Forbid();
                }

                var model = new UpdateUserCommandDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    Hobbies = user.Hobbies
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user for edit");
                TempData["ErrorMessage"] = "Error loading user. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize]
        [HttpPost("/users/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateUserCommandDto model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            // Use FluentValidation
            var validationResult = await _updateUserValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                // Add FluentValidation errors to ModelState
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Authorization: allow only owner or admin
                    var isAdmin = User.IsInRole("Admin");
                    var userIdClaim = User.FindFirst("userId")?.Value;
                    var currentUserId = int.TryParse(userIdClaim, out var parsedId) ? parsedId : 0;
                    if (!isAdmin && currentUserId != id)
                    {
                        return Forbid();
                    }

                    await _userService.UpdateAsync(model);
                    TempData["SuccessMessage"] = "User updated successfully!";
                    _logger.LogInformation("User {Id} updated by {User}", id, User.Identity?.Name);
                    return RedirectToAction(nameof(Details), new { id = id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating user {Id}", id);
                    ModelState.AddModelError("", "Error updating user. Please try again.");
                }
            }

            return View(model);
        }

        [Authorize]
        [HttpGet("/users/details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user details");
                TempData["ErrorMessage"] = "Error loading user details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("/users/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _userService.DeleteAsync(id);
                TempData["SuccessMessage"] = "User deleted successfully!";
                _logger.LogInformation("User {Id} deleted by {User}", id, User.Identity?.Name);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {Id}", id);
                TempData["ErrorMessage"] = "Error deleting user. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
