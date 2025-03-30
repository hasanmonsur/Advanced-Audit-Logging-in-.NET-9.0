using AuditLoggingWebAPI.Middlewares;
using AuditLoggingWebAPI.Models;

namespace AuditLoggingWebAPI.Extensions
{
    // Extensions/AuditLogExtensions.cs
    public static class AuditLogExtensions
    {
        public static IServiceCollection AddAuditLogging(this IServiceCollection services,
            Action<AuditLogOptions> configureOptions)
        {
            services.Configure(configureOptions);
            return services;
        }

        public static IApplicationBuilder UseAuditLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuditLogMiddleware>();
        }
    }
}
