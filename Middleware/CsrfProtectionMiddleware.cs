using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebERP.Middleware
{
    public class CsrfProtectionMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly string[] AllowedOrigins = new[]
        {
            "https://localhost:7215",
            "http://localhost:5058",
            "https://localhost:7979",
            "https://localhost:6868"
        };

        public CsrfProtectionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var method = context.Request.Method;

            // Apply CSRF check for state-changing requests (POST, PUT, DELETE)
            if (HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsDelete(method))
            {
                var path = context.Request.Path.Value?.ToLower() ?? "";

                // Skip CSRF checks for Swagger UI and login/logout endpoints
                if (!path.StartsWith("/swagger") && 
                    !path.Contains("/api/auth/login") && 
                    !path.Contains("/api/auth/logout"))
                {
                    var origin = context.Request.Headers["Origin"].ToString();
                    if (string.IsNullOrEmpty(origin))
                    {
                        origin = context.Request.Headers["Referer"].ToString();
                    }

                    if (string.IsNullOrEmpty(origin) || !IsTrustedOrigin(origin))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new
                        {
                            success = false,
                            message = "CSRF Verification Failed: Request Origin or Referer is not trusted."
                        });
                        return;
                    }
                }
            }

            await _next(context);
        }

        private bool IsTrustedOrigin(string origin)
        {
            return AllowedOrigins.Any(allowed => origin.StartsWith(allowed, StringComparison.OrdinalIgnoreCase));
        }
    }
}
