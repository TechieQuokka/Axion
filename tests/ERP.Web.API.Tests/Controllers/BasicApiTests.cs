using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using FluentAssertions;
using System.Net;

namespace ERP.Web.API.Tests.Controllers
{
    public class BasicApiTests : IClassFixture<TestApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public BasicApiTests(TestApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_ApiInfo_Should_Return_Success()
        {
            // Act
            var response = await _client.GetAsync("/api/info");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("ERP API");
        }

        [Fact]
        public async Task Get_Health_Should_Return_Success()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}