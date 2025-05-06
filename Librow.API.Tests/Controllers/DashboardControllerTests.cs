using FluentAssertions;
using Librow.API.Controllers;
using Librow.Application.Models;
using Librow.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Librow.API.Tests.Controllers
{
    public class DashboardControllerTests
    {
        private readonly Mock<IDashboardService> _dashboardServiceMock;
        private readonly DashboardController _controller;

        public DashboardControllerTests()
        {
            _dashboardServiceMock = new Mock<IDashboardService>();
            _controller = new DashboardController(_dashboardServiceMock.Object);
        }

        [Fact]
        public async Task GetSummaryInfo_ShouldReturnOkResponse()
        {
            // Arrange
            var expectedResult = Result<object>.SuccessWithBody(new { Summary = "Info" });
            _dashboardServiceMock.Setup(service => service.GetSummaryInfo())
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetSummaryInfo();

            // Assert
            var okResult = result as ObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().Be(expectedResult);
        }

        [Fact]
        public async Task GetPopularBooks_ShouldReturnOkResponse()
        {
            // Arrange
            var top = 5;
            var expectedResult = Result<object>.SuccessWithBody(new { Books = "Popular Books Data" });

            _dashboardServiceMock.Setup(service => service.GetPopularBooks(top))
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetPopularBooks(top);

            // Assert
            var okResult = result as ObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().Be(expectedResult);
        }

        [Fact]
        public async Task GetRequestAnalysis_ShouldReturnOkResponse()
        {
            // Arrange
            var expectedResult = Result<object>.SuccessWithBody(new { Analysis = "Request Analysis Data" });
            _dashboardServiceMock.Setup(service => service.GetRequestAnalysis())
                                 .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetRequestAnalysis();

            // Assert
            var okResult = result as ObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().Be(expectedResult);
        }
    }
}
