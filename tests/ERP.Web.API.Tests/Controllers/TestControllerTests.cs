using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;

namespace ERP.Web.API.Tests.Controllers
{
    public class TestControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public TestControllerTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _client = factory.CreateClient();
            _output = output;
        }

        [Fact]
        public async Task Get_Test_Should_Return_Success()
        {
            // Act
            var response = await _client.GetAsync("/api/test");

            // Assert - BeSuccessful() 대신 명확한 방식 사용
            _output.WriteLine($"Response Status: {response.StatusCode}");
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response Content: {content}");

            // 성공 상태 코드 확인 (200-299)
            Assert.True(response.IsSuccessStatusCode,
                $"Expected success status code but got {response.StatusCode}. Content: {content}");

            content.Should().Contain("API is working");
        }

        [Fact]
        public async Task Get_ApiInfo_Should_Work()
        {
            // Act
            var response = await _client.GetAsync("/api/info");

            // Assert
            _output.WriteLine($"API Info Status: {response.StatusCode}");
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"API Info Content: {content}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("ERP API");
        }

        [Fact]
        public async Task Get_Projects_Debug()
        {
            // Act
            var response = await _client.GetAsync("/api/projects");

            // Assert - 디버깅 목적
            _output.WriteLine($"Projects Status: {response.StatusCode}");
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Projects Content: {content}");

            // 응답이 와야 함 (성공이든 실패든)
            response.Should().NotBeNull();

            // 상태 코드별 분석
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                _output.WriteLine("서버 내부 오류 발생!");
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _output.WriteLine("인증 오류 발생!");
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _output.WriteLine("엔드포인트를 찾을 수 없음!");
            }
        }
    }
}