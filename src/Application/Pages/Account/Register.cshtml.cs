using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace HobbyApp.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    [BindProperty]
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    [StringLength(100)]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public List<HobbyInputModel> Hobbies { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;
    public string SuccessMessage { get; set; } = string.Empty;

    public RegisterModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            // Convert hobbies to the expected format
            var hobbyDtos = Hobbies.Select(h => new
            {
                Name = h.Name,
                Level = h.Level
            }).ToList();

            var registerRequest = new
            {
                Username = this.Username,
                FullName = this.FullName,
                Email = this.Email,
                Password = this.Password,
                Hobbies = hobbyDtos
            };

            var json = JsonSerializer.Serialize(registerRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{baseUrl}/api/auth/register", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent);

                if (loginResponse != null)
                {
                    // Store token in session
                    HttpContext.Session.SetString("JWTToken", loginResponse.Token);
                    HttpContext.Session.SetString("Username", loginResponse.Username);
                    HttpContext.Session.SetString("FullName", loginResponse.FullName);

                    SuccessMessage = "Registration successful! Welcome!";
                    TempData["SuccessMessage"] = SuccessMessage;

                    return RedirectToPage("/Users/Index");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent);
                ErrorMessage = errorResponse?.Message ?? "Registration failed. Please try again.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "An error occurred while registering. Please try again.";
        }

        return Page();
    }

    public IActionResult OnGet()
    {
        // If already logged in, redirect to users page
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("JWTToken")))
        {
            return RedirectToPage("/Users/Index");
        }

        return Page();
    }

    public class HobbyInputModel
    {
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
    }

    private class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    private class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}

