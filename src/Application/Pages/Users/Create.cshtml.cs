using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace HobbyApp.Pages.Users;

public class CreateModel : PageModel
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

    public CreateModel(IHttpClientFactory httpClientFactory)
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
            var token = HttpContext.Session.GetString("JWTToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            // Convert hobbies to the expected format
            var hobbyDtos = Hobbies.Select(h => new
            {
                Name = h.Name,
                Level = h.Level
            }).ToList();

            var createRequest = new
            {
                Username = this.Username,
                FullName = this.FullName,
                Email = this.Email,
                Password = this.Password,
                Hobbies = hobbyDtos
            };

            var json = JsonSerializer.Serialize(createRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{baseUrl}/api/users", content);

            if (response.IsSuccessStatusCode)
            {
                SuccessMessage = "User created successfully!";
                TempData["SuccessMessage"] = SuccessMessage;

                // Reset form
                Username = string.Empty;
                FullName = string.Empty;
                Email = string.Empty;
                Password = string.Empty;
                Hobbies = new();

                return RedirectToPage("/Users/Index");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent);
                ErrorMessage = errorResponse?.Message ?? "Failed to create user. Please try again.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "An error occurred while creating the user. Please try again.";
        }

        return Page();
    }

    public IActionResult OnGet()
    {
        // Check if user is logged in
        var token = HttpContext.Session.GetString("JWTToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToPage("/Account/Login");
        }

        return Page();
    }

    public class HobbyInputModel
    {
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
    }

    private class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
