using Hangfire.Dashboard;

namespace DBIntegrationHub.Presentation.Extensions;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        // Kullanıcı authenticated ve Admin rolüne sahip mi?
        return httpContext.User.Identity?.IsAuthenticated == true &&
               httpContext.User.IsInRole("Admin");
    }
}

