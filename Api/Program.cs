using Api.Extensions;
using Api.Middleware;
using Shared.Configuration;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection("Security"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));
builder.Services.Configure<OpenTelemetrySettings>(builder.Configuration.GetSection("OpenTelemetry"));
builder.Services.AddSwaggerConfiguration();
builder.Services.AddDatabaseConfiguration();
builder.Services.AddApplicationServices();
builder.Services.AddLoggingConfiguration(builder.Configuration);
builder.AddOpenTemlemetryConfiguration();
builder.Services.AddRateLimiterConfiguration();
builder.Services.AddAuthenticationConfiguration();
builder.Services.AddAuthorization();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<AuthenticationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
