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

// 테스트 프로젝트에서 internal 클래스 접근 허용
[assembly: InternalsVisibleTo("ERP.Web.API.Tests")]
[assembly: InternalsVisibleTo("ERP.IntegrationTests")]

var builder = WebApplication.CreateBuilder(args);

// Serilog 설정
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // API Exception Filter 등록
    options.Filters.Add<ApiExceptionFilterAttribute>();
})
.AddJsonOptions(options =>
{
    // Enum을 문자열로 직렬화
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    // Camel case 속성명 사용
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
        Description = "IT 서비스업 특화 ERP 시스템 API",
        Contact = new OpenApiContact
        {
            Name = "ERP Team",
            Email = "support@erp.com"
        }
    });

    // JWT Bearer 토큰 인증 설정
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

// CORS 설정 - appsettings.json의 AllowedOrigins 사용
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000", "https://localhost:3001" };

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // SignalR을 위해 필요
    });
});

// SignalR 서비스 추가
builder.Services.AddSignalR();

// Authentication & Authorization - appsettings.json의 JwtSettings 사용
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
            ClockSkew = TimeSpan.FromMinutes(5) // 토큰 만료 시간 허용 오차
        };

        // SignalR JWT 인증을 위한 설정
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

// Serilog 미들웨어 추가
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

// CORS 적용 (Authentication 전에)
app.UseCors();

// Tenant Middleware (Authentication 전에)
app.UseTenantMiddleware();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Static Files (파일 업로드를 위해 필요)
app.UseStaticFiles();

// Controllers 매핑
app.MapControllers();

// SignalR Hubs 매핑
app.MapHub<ProjectHub>("/hubs/project");

// Hangfire Dashboard (개발환경에서만)
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter() }
    });
}

// Health Check 엔드포인트
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0",
    environment = app.Environment.EnvironmentName
}));

// API Info 엔드포인트
app.MapGet("/api/info", () => Results.Ok(new
{
    name = "ERP API",
    version = "1.0.0",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow,
    description = "IT 서비스업 특화 ERP 시스템"
}));

try
{
    Log.Information("ERP API 서버를 시작합니다...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ERP API 서버 시작 중 오류가 발생했습니다");
}
finally
{
    Log.CloseAndFlush();
}

// Program 클래스를 public으로 명시적 선언 (대안)
public partial class Program { }

// Hangfire 인증 필터 (개발용)
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // 개발환경에서는 모든 접근 허용
        // 운영환경에서는 실제 인증 로직 구현 필요
        return true;
    }
}