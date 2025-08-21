using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using HobbyApp.Infrastructure.Repositories.Interfaces;

namespace HobbyApp.Infrastructure.Authorization;

public class RoleAuthorizationAttribute : ActionFilterAttribute
{
    private readonly string _requiredRole;

    public RoleAuthorizationAttribute(string requiredRole)
    {
        _requiredRole = requiredRole;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var unitOfWork = context.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
        var hasRole = await unitOfWork.UserRoles.UserHasRoleAsync(userId, _requiredRole);

        if (!hasRole)
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
