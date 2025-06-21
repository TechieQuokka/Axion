using ERP.Application.Common.Interfaces;
using ERP.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace ERP.Web.API.Tests
{
    public class TestApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // 실제 DB 연결 제거
                var dbDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (dbDescriptor != null)
                    services.Remove(dbDescriptor);

                // Redis 연결 제거
                var redisDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IDistributedCache));
                if (redisDescriptor != null)
                    services.Remove(redisDescriptor);

                // CurrentUserService Mock
                var currentUserDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ICurrentUserService));
                if (currentUserDescriptor != null)
                    services.Remove(currentUserDescriptor);

                var mockCurrentUserService = new Mock<ICurrentUserService>();
                mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
                mockCurrentUserService.Setup(x => x.CompanyId).Returns(1);

                // 올바른 타입으로 Mock 설정
                mockCurrentUserService.Setup(x => x.IdentityUserId).Returns("test-identity-user-id");  // string
                mockCurrentUserService.Setup(x => x.BusinessUserId).Returns(1);  // int
                mockCurrentUserService.Setup(x => x.UserId).Returns(1);  // int (BusinessUserId와 동일)
                mockCurrentUserService.Setup(x => x.UserName).Returns("Test User");

                // 추가 메서드들도 Mock 설정
                mockCurrentUserService.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(false);
                mockCurrentUserService.Setup(x => x.HasPermission(It.IsAny<string>())).Returns(true);

                services.AddSingleton(mockCurrentUserService.Object);

                // DateTime Mock
                var dateTimeDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IDateTime));
                if (dateTimeDescriptor != null)
                    services.Remove(dateTimeDescriptor);

                var mockDateTime = new Mock<IDateTime>();
                mockDateTime.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);
                mockDateTime.Setup(x => x.Now).Returns(DateTime.Now);
                services.AddSingleton(mockDateTime.Object);

                // InMemory DB와 Cache로 대체
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                });

                services.AddMemoryCache();
                services.AddSingleton<IDistributedCache, MemoryDistributedCache>();

                // 로깅 레벨 조정
                services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
            });

            builder.UseEnvironment("Testing");
        }
    }
}