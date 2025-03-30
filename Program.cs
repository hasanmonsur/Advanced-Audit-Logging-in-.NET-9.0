using AuditLoggingWebAPI.Extensions;
using AuditLoggingWebAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add services
builder.Services.AddControllers();
// Configure audit logging
builder.Services.AddAuditLogging(options =>
{
    options.LogResponseBody = true;
    options.SensitiveFields.Add("ssn");
    options.ExcludePaths.Add("/swagger");
});

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.JsonWriterOptions = new System.Text.Json.JsonWriterOptions
    {
        Indented = true
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll"); // Apply CORS policy

// Add audit logging middleware
app.UseHttpsRedirection();
app.UseAuthorization();

// Register the audit logging middleware
app.UseAuditLogging();

app.MapControllers();

app.Run();