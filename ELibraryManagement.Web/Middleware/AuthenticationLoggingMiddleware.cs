using ELibraryManagement.Web.Services;

namespace ELibraryManagement.Web.Middleware
{
    public class AuthenticationLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAuthApiService authService)
        {
            // Log authentication state for debugging
            if (context.Request.Path.StartsWithSegments("/Account") ||
                context.Request.Path.StartsWithSegments("/Book"))
            {
                var isAuth = authService.IsAuthenticated();
                var token = authService.GetCurrentUserToken();
                var userName = authService.GetCurrentUserName();

                System.Diagnostics.Debug.WriteLine($"[AUTH DEBUG] Path: {context.Request.Path}, IsAuth: {isAuth}, HasToken: {!string.IsNullOrEmpty(token)}, UserName: {userName}");

                // Check session data
                if (context.Session != null)
                {
                    var sessionToken = context.Session.GetString("AuthToken");
                    var sessionUserName = context.Session.GetString("UserName");
                    System.Diagnostics.Debug.WriteLine($"[SESSION DEBUG] SessionToken: {!string.IsNullOrEmpty(sessionToken)}, SessionUserName: {sessionUserName}");
                }
            }

            await _next(context);
        }
    }
}