using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace HobbyApp.Pages.Users;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public List<UserListItemDto> Users { get; set; } = new();
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string Search { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Size);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> OnGetAsync(int page = 1, int size = 10, string search = "")
    {
        // Check if user is logged in
        var token = HttpContext.Session.GetString("JWTToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToPage("/Account/Login");
        }

        Page = page;
        Size = size;
        Search = search;

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var url = $"{baseUrl}/api/users?page={page}&size={size}";
            if (!string.IsNullOrEmpty(search))
            {
                url += $"&search={Uri.EscapeDataString(search)}";
            }

            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedResult<UserListItemDto>>>(content);

                if (apiResponse?.Data != null)
                {
                    Users = apiResponse.Data.Items;
                    TotalCount = apiResponse.Data.TotalCount;
                    Page = apiResponse.Data.Page;
                    Size = apiResponse.Data.Size;
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToPage("/Account/Login");
            }
        }
        catch (Exception)
        {
            // Handle error silently for now
        }

        return Page();
    }

    public async Task<IActionResult> OnPostLogoutAsync()
    {
        HttpContext.Session.Clear();
        return RedirectToPage("/Account/Login");
    }

    public class UserListItemDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int HobbyCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    private class PaginatedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
    }

    private class ApiResponse<T>
    {
        public T? Data { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Error { get; set; }
    }
}

