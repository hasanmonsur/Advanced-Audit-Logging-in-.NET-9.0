using AuditLoggingWebAPI.Models;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace AuditLoggingWebAPI.Middlewares
{
    // Middleware/AuditLogMiddleware.cs
    public class AuditLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AuditLogOptions _options;
        private readonly ILogger<AuditLogMiddleware> _logger;

        public AuditLogMiddleware(
            RequestDelegate next,
            IOptions<AuditLogOptions> options,
            ILogger<AuditLogMiddleware> logger)
        {
            _next = next;
            _options = options.Value;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip logging for excluded paths
            if (_options.ExcludePaths.Any(p => context.Request.Path.StartsWithSegments(p)))
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var auditEntry = new AuditLogEntry
            {
                Method = context.Request.Method,
                Path = context.Request.Path,
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                Timestamp = DateTime.UtcNow
            };

            // Capture user info if authenticated
            if (context.User.Identity?.IsAuthenticated == true)
            {
                auditEntry.UserId = context.User.FindFirst("sub")?.Value;
                auditEntry.UserName = context.User.Identity.Name;
            }

            // Capture route values
            auditEntry.RouteValues = context.Request.RouteValues
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString());

            // Capture query string
            auditEntry.QueryString = context.Request.Query
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());

            // Capture request headers if enabled
            if (_options.LogRequestHeaders)
            {
                auditEntry.AdditionalData ??= new Dictionary<string, object>();
                auditEntry.AdditionalData["RequestHeaders"] = context.Request.Headers
                    .ToDictionary(h => h.Key, h => h.Value.ToString());
            }

            // Capture request body if enabled
            if (_options.LogRequestBody && context.Request.ContentLength > 0 &&
                context.Request.ContentLength < _options.MaxRequestBodyLength)
            {
                context.Request.EnableBuffering();
                var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
                context.Request.Body.Position = 0;

                auditEntry.RequestBody = MaskSensitiveData(requestBody);
            }

            // Intercept the response to capture details
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);
                stopwatch.Stop();

                auditEntry.Duration = stopwatch.Elapsed;
                auditEntry.ResponseStatusCode = context.Response.StatusCode;
                auditEntry.IsSuccess = context.Response.StatusCode < 400;

                // Capture response body if enabled
                if (_options.LogResponseBody &&
                    context.Response.ContentLength < _options.MaxResponseBodyLength)
                {
                    responseBody.Seek(0, SeekOrigin.Begin);
                    var responseBodyContent = await new StreamReader(responseBody).ReadToEndAsync();
                    responseBody.Seek(0, SeekOrigin.Current);

                    auditEntry.ResponseBody = MaskSensitiveData(responseBodyContent);
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                auditEntry.Duration = stopwatch.Elapsed;
                auditEntry.ResponseStatusCode = 500;
                auditEntry.IsSuccess = false;
                auditEntry.Errors = new List<string> { ex.Message };
                _logger.LogError(ex, "Request processing error");

                // Re-throw the exception for the error handling middleware
                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;

                // Log the audit entry
                LogAuditEntry(auditEntry);
            }
        }

        private string MaskSensitiveData(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            foreach (var field in _options.SensitiveFields)
            {
                var pattern = $"\"{field}\":\\s*\"([^\"]*)\"";
                input = Regex.Replace(input, pattern, $"\"{field}\":\"*****\"",
                    RegexOptions.IgnoreCase);
            }
            return input;
        }

        private void LogAuditEntry(AuditLogEntry entry)
        {
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["AuditId"] = entry.Id,
                ["UserId"] = entry.UserId ?? "anonymous",
                ["Path"] = entry.Path,
                ["Method"] = entry.Method
            }))
            {
                _logger.LogInformation("Audit log: {@AuditEntry}", entry);
            }
        }
    }
}
