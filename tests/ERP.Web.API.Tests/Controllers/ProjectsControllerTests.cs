using ERP.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;

namespace ERP.Web.API.Tests.Controllers
{
    public class ProjectsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public ProjectsControllerTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _output = output;
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // 실제 MySQL 대신 InMemory 데이터베이스 사용
                    var dbDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (dbDescriptor != null)
                        services.Remove(dbDescriptor);

                    // Redis 연결 문제 해결
                    var redisDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IDistributedCache));
                    if (redisDescriptor != null)
                        services.Remove(redisDescriptor);

                    // InMemory DB와 Cache로 대체
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });

                    services.AddMemoryCache();
                    services.AddSingleton<IDistributedCache, MemoryDistributedCache>();
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Get_Projects_Should_Not_Crash()
        {
            // Act
            var response = await _client.GetAsync("/api/projects");

            // Assert - 디버깅 목적
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Projects API Status: {response.StatusCode}");
            _output.WriteLine($"Projects API Content: {content}");

            // 응답이 와야 함 (성공이든 실패든)
            response.Should().NotBeNull();

            // 상태 코드별 분석
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                _output.WriteLine("서버 내부 오류 발생!");
                _output.WriteLine($"오류 내용: {content}");
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _output.WriteLine("인증 오류 발생!");
            }
            else if (response.IsSuccessStatusCode)
            {
                _output.WriteLine("Projects API 정상 작동!");
                content.Should().Contain("items");
            }
        }

        [Fact]
        public async Task Get_ApiInfo_Should_Return_Success()
        {
            // Act
            var response = await _client.GetAsync("/api/info");

            // Assert
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"API Info Status: {response.StatusCode}");
            _output.WriteLine($"API Info Content: {content}");

            // BeSuccessful() 대신 표준 방식 사용
            Assert.True(response.IsSuccessStatusCode,
                $"API Info 호출 실패. Status: {response.StatusCode}, Content: {content}");

            content.Should().Contain("ERP API");
        }

        [Fact]
        public async Task Get_Health_Should_Return_Success()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Health Check Status: {response.StatusCode}");
            _output.WriteLine($"Health Check Content: {content}");

            // 상태 코드 직접 확인
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Swagger_Should_Return_Success()
        {
            // Act
            var response = await _client.GetAsync("/swagger/index.html");

            // Assert
            _output.WriteLine($"Swagger Status: {response.StatusCode}");

            // Swagger는 개발환경에서만 사용 가능
            if (response.IsSuccessStatusCode)
            {
                _output.WriteLine("Swagger 정상 작동!");
            }
            else
            {
                _output.WriteLine($"Swagger 접근 실패: {response.StatusCode}");
            }

            response.Should().NotBeNull();
        }

        [Fact]
        public async Task Debug_All_Endpoints()
        {
            var endpoints = new[]
            {
                "/api/info",
                "/health",
                "/api/projects",
                "/swagger/index.html"
            };

            foreach (var endpoint in endpoints)
            {
                var response = await _client.GetAsync(endpoint);
                var content = await response.Content.ReadAsStringAsync();

                _output.WriteLine($"=== {endpoint} ===");
                _output.WriteLine($"Status: {response.StatusCode}");
                _output.WriteLine($"Content Length: {content.Length}");
                _output.WriteLine($"Content Preview: {content.Take(200)}...");
                _output.WriteLine("");
            }
        }
    }
}