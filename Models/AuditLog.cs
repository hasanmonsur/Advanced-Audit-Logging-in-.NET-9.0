namespace AuditLoggingWebAPI.Models
{
    // Models/AuditLogEntry.cs
    public class AuditLogEntry
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? IpAddress { get; set; }
        public string? Action { get; set; }
        public string? Controller { get; set; }
        public string? Method { get; set; }
        public string? Path { get; set; }
        public Dictionary<string, string?>? RouteValues { get; set; }
        public Dictionary<string, string?>? QueryString { get; set; }
        public string? RequestBody { get; set; }
        public int? ResponseStatusCode { get; set; }
        public string? ResponseBody { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsSuccess { get; set; }
        public List<string>? Errors { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    // Models/AuditLogOptions.cs
    public class AuditLogOptions
    {
        public bool LogRequestHeaders { get; set; } = true;
        public bool LogResponseHeaders { get; set; } = false;
        public bool LogRequestBody { get; set; } = true;
        public bool LogResponseBody { get; set; } = false;
        public int MaxRequestBodyLength { get; set; } = 4096; // 4KB
        public int MaxResponseBodyLength { get; set; } = 4096; // 4KB
        public List<string> SensitiveFields { get; set; } = new() { "password", "creditcard", "token" };
        public List<string> ExcludePaths { get; set; } = new() { "/health", "/favicon.ico" };
    }
}
