using System.Security.Claims;
using System.Text;

using Yarp.ReverseProxy;

using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using Shared.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
    ?? builder.Configuration["Jwt:SecretKey"] 
    ?? throw new InvalidOperationException("JWT_SECRET_KEY not configured");

var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
    ?? builder.Configuration["Jwt:Issuer"] 
    ?? "StockOrchestra";

var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
    ?? builder.Configuration["Jwt:Audience"] 
    ?? "StockOrchestra-Api";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddMemoryCache();
builder.Services.Configure<ClientRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = 60
        }
    };
});
builder.Services.AddSingleton<IClientPolicyStore, MemoryCacheClientPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS") 
    ?? builder.Configuration["Cors:AllowedOrigins"] 
    ?? "http://localhost:3000";

builder.Services.AddCors(options =>
{
    options.AddPolicy("SecureFrontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins.Split(','))
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddHealthChecks();
builder.Logging.AddConsole();

var app = builder.Build();

app.UseIpRateLimiting();
app.UseCors("SecureFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline.Use(async (context, next) =>
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = context.User.FindFirst(ClaimTypes.Name)?.Value;
            var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value);
            
            context.Request.Headers["X-User-Id"] = userId ?? "";
            context.Request.Headers["X-Username"] = username ?? "";
            context.Request.Headers["X-User-Roles"] = string.Join(",", roles);
        }
        
        await next();
    });
});

app.MapHealthChecks("/health");
app.MapGet("/gateway-info", () => "StockOrchestra API Gateway v1.0");

app.Run();