using ERP.Application;
using ERP.Infrastructure;
using ERP.Web.API.Filters;
using ERP.Web.API.Hubs;
using ERP.Web.API.Middleware;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;

// �׽�Ʈ ������Ʈ���� internal Ŭ���� ���� ���
[assembly: InternalsVisibleTo("ERP.Web.API.Tests")]
[assembly: InternalsVisibleTo("ERP.IntegrationTests")]

var builder = WebApplication.CreateBuilder(args);

// Serilog ����
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // API Exception Filter ���
    options.Filters.Add<ApiExceptionFilterAttribute>();
})
.AddJsonOptions(options =>
{
    // Enum�� ���ڿ��� ����ȭ
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    // Camel case �Ӽ��� ���
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ERP API",
        Version = "v1",
        Description = "IT ���񽺾� Ưȭ ERP �ý��� API",
        Contact = new OpenApiContact
        {
            Name = "ERP Team",
            Email = "support@erp.com"
        }
    });

    // JWT Bearer ��ū ���� ����
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Application & Infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// CORS ���� - appsettings.json�� AllowedOrigins ���
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000", "https://localhost:3001" };

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // SignalR�� ���� �ʿ�
    });
});

// SignalR ���� �߰�
builder.Services.AddSignalR();

// Authentication & Authorization - appsettings.json�� JwtSettings ���
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "default-secret-key-that-is-at-least-32-characters-long";
var issuer = jwtSettings["Issuer"] ?? "ERP-System";
var audience = jwtSettings["Audience"] ?? "erp-api";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.FromMinutes(5) // ��ū ���� �ð� ��� ����
        };

        // SignalR JWT ������ ���� ����
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Serilog �̵���� �߰�
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ERP API v1");
        c.RoutePrefix = "swagger"; // https://localhost:7051/swagger
    });
}

// HTTPS Redirection
app.UseHttpsRedirection();

// CORS ���� (Authentication ����)
app.UseCors();

// Tenant Middleware (Authentication ����)
app.UseTenantMiddleware();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Static Files (���� ���ε带 ���� �ʿ�)
app.UseStaticFiles();

// Controllers ����
app.MapControllers();

// SignalR Hubs ����
app.MapHub<ProjectHub>("/hubs/project");

// Hangfire Dashboard (����ȯ�濡����)
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter() }
    });
}

// Health Check ��������Ʈ
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0",
    environment = app.Environment.EnvironmentName
}));

// API Info ��������Ʈ
app.MapGet("/api/info", () => Results.Ok(new
{
    name = "ERP API",
    version = "1.0.0",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow,
    description = "IT ���񽺾� Ưȭ ERP �ý���"
}));

try
{
    Log.Information("ERP API ������ �����մϴ�...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ERP API ���� ���� �� ������ �߻��߽��ϴ�");
}
finally
{
    Log.CloseAndFlush();
}

// Program Ŭ������ public���� ����� ���� (���)
public partial class Program { }

// Hangfire ���� ���� (���߿�)
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // ����ȯ�濡���� ��� ���� ���
        // �ȯ�濡���� ���� ���� ���� ���� �ʿ�
        return true;
    }
}