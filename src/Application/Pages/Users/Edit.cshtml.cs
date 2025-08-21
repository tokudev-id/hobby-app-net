using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace HobbyApp.Pages.Users;

public class EditModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    [BindProperty(SupportsGet = true)]
    public int UserId { get; set; }

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
    public List<HobbyInputModel> Hobbies { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;
    public string SuccessMessage { get; set; } = string.Empty;

    public EditModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        UserId = id;

        // Check if user is logged in
        var token = HttpContext.Session.GetString("JWTToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToPage("/Account/Login");
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var response = await client.GetAsync($"{baseUrl}/api/users/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<UserDetailDto>>(content);

                if (apiResponse?.Data != null)
                {
                    Username = apiResponse.Data.Username;
                    FullName = apiResponse.Data.FullName;
                    Email = apiResponse.Data.Email;
                    Hobbies = apiResponse.Data.Hobbies.Select(h => new HobbyInputModel
                    {
                        Name = h.Name,
                        Level = h.Level
                    }).ToList();
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToPage("/Account/Login");
            }
            else
            {
                ErrorMessage = "User not found.";
            }
        }
        catch (Exception)
        {
            ErrorMessage = "An error occurred while loading the user.";
        }

        return Page();
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

            var updateRequest = new
            {
                Id = UserId,
                Username = this.Username,
                FullName = this.FullName,
                Email = this.Email,
                Hobbies = hobbyDtos
            };

            var json = JsonSerializer.Serialize(updateRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{baseUrl}/api/users/{UserId}", content);

            if (response.IsSuccessStatusCode)
            {
                SuccessMessage = "User updated successfully!";
                TempData["SuccessMessage"] = SuccessMessage;

                return RedirectToPage("/Users/Details", new { id = UserId });
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent);
                ErrorMessage = errorResponse?.Message ?? "Failed to update user. Please try again.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "An error occurred while updating the user. Please try again.";
        }

        return Page();
    }

    public class HobbyInputModel
    {
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
    }

    private class UserDetailDto
    {
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<HobbyDto> Hobbies { get; set; } = new();
    }

    private class HobbyDto
    {
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
    }

    private class ApiResponse<T>
    {
        public T? Data { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Error { get; set; }
    }

    private class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
