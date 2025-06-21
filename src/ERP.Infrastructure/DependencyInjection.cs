using ERP.Application.Common.Interfaces;
using ERP.Infrastructure.Data;
using ERP.Infrastructure.Services;
using ERP.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Hangfire;
using Hangfire.MySql;

namespace ERP.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(
                    configuration.GetConnectionString("DefaultConnection"),
                    ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection")),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

            // Identity
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;

                // Sign-in settings
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Services - Scoped로 변경 (DB 접근 필요)
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IFileStorageService, FileStorageService>();

            // DateTime service
            services.AddTransient<IDateTime, DateTimeService>();

            // HttpContextAccessor (CurrentUserService에 필요)
            services.AddHttpContextAccessor();

            // Caching - Redis
            var redisConnectionString = configuration.GetConnectionString("Redis");
            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = "ERP";
                });
            }
            else
            {
                // Redis가 없으면 메모리 캐시 사용
                services.AddMemoryCache();
                services.AddSingleton<IDistributedCache, MemoryDistributedCache>();
            }

            // Background Services - Hangfire
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseStorage(new MySqlStorage(
                    configuration.GetConnectionString("DefaultConnection"),
                    new MySqlStorageOptions
                    {
                        QueuePollInterval = TimeSpan.FromSeconds(10),
                        JobExpirationCheckInterval = TimeSpan.FromHours(1),
                        CountersAggregateInterval = TimeSpan.FromMinutes(5),
                        PrepareSchemaIfNecessary = true,
                        DashboardJobListLimit = 25000,
                        TransactionTimeout = TimeSpan.FromMinutes(1),
                        TablesPrefix = "Hangfire"
                    })));

            services.AddHangfireServer(options =>
            {
                options.WorkerCount = Environment.ProcessorCount * 2;
                options.Queues = new[] { "default", "critical", "background" };
                options.ServerName = Environment.MachineName;
            });

            return services;
        }
    }
}