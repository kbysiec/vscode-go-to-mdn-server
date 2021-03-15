using System.Security.Claims;
using Coravel.Pro.Features.Auth.Interfaces;
using Microsoft.AspNetCore.Http;

namespace WebApp.Scheduler
{
    public class DashboardAccessValidator : IHasPermission
    {
        public bool HasPermission(HttpRequest request, ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.Identity.IsAuthenticated && claimsPrincipal.HasClaim("CanAccess", "true");
        }
    }
}
