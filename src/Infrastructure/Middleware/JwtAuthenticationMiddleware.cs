using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HobbyApp.Infrastructure.Security;

namespace HobbyApp.Infrastructure.Middleware;

public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public JwtAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check for JWT token in HttpOnly cookie
        if (context.Request.Cookies.TryGetValue("auth_token", out var token))
        {
            // Validate the token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!);

            try
            {
                var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JwtSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["JwtSettings:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                // Set the user principal
                context.User = claimsPrincipal;

                // Check if token is about to expire (within 5 minutes) and try to refresh
                var expClaim = claimsPrincipal.FindFirst("exp");
                if (expClaim != null && long.TryParse(expClaim.Value, out var exp))
                {
                    var expiryDate = DateTimeOffset.FromUnixTimeSeconds(exp);
                    if (expiryDate < DateTimeOffset.UtcNow.AddMinutes(5))
                    {
                        await TryRefreshTokenAsync(context);
                    }
                }
            }
            catch (SecurityTokenExpiredException)
            {
                // Token expired, try to refresh
                await TryRefreshTokenAsync(context);
            }
            catch (Exception)
            {
                // Token invalid, clear the cookie
                context.Response.Cookies.Delete("auth_token");
                context.Response.Cookies.Delete("refresh_token");
            }
        }

        await _next(context);
    }

    private async Task TryRefreshTokenAsync(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
        {
            var tokenService = context.RequestServices.GetRequiredService<JwtTokenService>();

            if (tokenService.ValidateRefreshToken(refreshToken) != null)
            {
                // Get the original user info from the expired token
                if (context.Request.Cookies.TryGetValue("auth_token", out var expiredToken))
                {
                    try
                    {
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var jwtToken = tokenHandler.ReadJwtToken(expiredToken);

                        var username = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                        var email = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
                        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
                        var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);

                        if (username != null && email != null && int.TryParse(userIdClaim, out var userId))
                        {
                            // Generate new tokens
                            var newAccessToken = tokenService.GenerateToken(username, email, userId, roleClaims);
                            var newRefreshToken = tokenService.GenerateRefreshToken();

                            // Set new cookies
                            SetAuthCookies(context.Response, newAccessToken, newRefreshToken);

                            // Set the new user principal
                            var newClaimsPrincipal = tokenHandler.ValidateToken(newAccessToken, new TokenValidationParameters
                            {
                                ValidateIssuerSigningKey = true,
                                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!)),
                                ValidateIssuer = true,
                                ValidIssuer = _configuration["JwtSettings:Issuer"],
                                ValidateAudience = true,
                                ValidAudience = _configuration["JwtSettings:Audience"],
                                ValidateLifetime = false, // Don't validate lifetime as we're creating a new token
                                ClockSkew = TimeSpan.Zero
                            }, out _);

                            context.User = newClaimsPrincipal;
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error refreshing token: {ex.Message}");
                    }
                }
            }

            // If refresh failed, clear cookies
            context.Response.Cookies.Delete("auth_token");
            context.Response.Cookies.Delete("refresh_token");
        }
    }

    private void SetAuthCookies(HttpResponse response, string accessToken, string refreshToken)
    {
        var isProduction = _configuration.GetValue<bool>("IsProduction");

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
}
