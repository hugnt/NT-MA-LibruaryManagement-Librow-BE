using Librow.Application.Models.Responses;
using Librow.Application.Services.Implement;
using Librow.Application.Services;
using Librow.Core.Entities;
using Librow.Infrastructure.Repositories.Base;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Librow.Application.Models;

namespace Librow.Application.Tests.Services.DashboardServiceTests;
public class DashboardServiceTests
{
    private readonly Mock<IRepository<Book>> _bookRepositoryMock;
    private readonly IDashboardService _dashboardService;

    public DashboardServiceTests()
    {
        _bookRepositoryMock = new Mock<IRepository<Book>>();
        _dashboardService = new DashboardService(_bookRepositoryMock.Object);
    }

    [Fact]
    public async Task GetSummaryInfo_ReturnsSuccessResult_WhenDataIsFound()
    {
        // Arrange
        var mockResult = new SummaryQueryResult
        {
            BookCount = 100,
            BookTotalQuantity = 500,
            BookTotalAvailable = 300,
            BookCategoryCount = 10,
            UserCount = 50,
            RequestCount = 200
        };

        _bookRepositoryMock
            .Setup(repo => repo.ExecuteRawSqlSingleAsync<SummaryQueryResult>(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(mockResult);

        // Act
        var result = await _dashboardService.GetSummaryInfo();
        var resultResponse = result.As<Result<List<DashboardModel>>>();

        // Assert
        resultResponse.IsSuccess.Should().BeTrue();
        resultResponse.Data.Should().NotBeEmpty();
        resultResponse.Data.Count.Should().Be(4);  // Check if there are 4 dashboard model items
        resultResponse.Data[0].Title.Should().Be("Books");
    }

    [Fact]
    public async Task GetSummaryInfo_ReturnsEmptyList_WhenQueryResultIsNull()
    {
        // Arrange
        _bookRepositoryMock
            .Setup(repo => repo.ExecuteRawSqlSingleAsync<SummaryQueryResult>(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((SummaryQueryResult)null);

        // Act
        var result = await _dashboardService.GetSummaryInfo();
        var resultResponse = result.As<Result<List<DashboardModel>>>();

        // Assert
        resultResponse.IsSuccess.Should().BeTrue();
        resultResponse.Data.Should().BeEmpty();  // Should return an empty list
    }

    [Fact]
    public async Task GetPopularBooks_ReturnsSuccessResult_WhenDataIsFound()
    {
        // Arrange
        var mockResult = new List<BookTrendQueryResult>
        {
            new BookTrendQueryResult { Id = Guid.NewGuid(), Title = "Book 1", TotalRequest = 50 },
            new BookTrendQueryResult { Id = Guid.NewGuid(), Title = "Book 2", TotalRequest = 45 }
        };

        _bookRepositoryMock
            .Setup(repo => repo.ExecuteRawSqlAsync<BookTrendQueryResult>(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(mockResult);

        // Act
        var result = await _dashboardService.GetPopularBooks();
        var resultResponse = result.As<Result<List<DashboardModel>>>();

        // Assert
        resultResponse.IsSuccess.Should().BeTrue();
        resultResponse.Data.Should().HaveCount(2);  // Check if the number of books returned is correct
    }

    [Fact]
    public async Task GetRequestAnalysis_ReturnsSuccessResult_WhenDataIsFound()
    {
        // Arrange
        var mockResult = new RequestStatusQueryResult
        {
            ApprovedNumber = 120,
            RejectedNumber = 30
        };

        _bookRepositoryMock
            .Setup(repo => repo.ExecuteRawSqlSingleAsync<RequestStatusQueryResult>(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(mockResult);

        // Act
        var result = await _dashboardService.GetRequestAnalysis();
        var resultResponse = result.As<Result<List<DashboardModel>>>();

        // Assert
        resultResponse.IsSuccess.Should().BeTrue();
        resultResponse.Data.Should().HaveCount(2);  // Should return 2 entries: Approved request and Rejected request
    }

    [Fact]
    public async Task GetRequestAnalysis_ReturnsEmptyList_WhenQueryResultIsNull()
    {
        // Arrange
        _bookRepositoryMock
            .Setup(repo => repo.ExecuteRawSqlSingleAsync<RequestStatusQueryResult>(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((RequestStatusQueryResult)null);

        // Act
        var result = await _dashboardService.GetRequestAnalysis();
        var resultResponse = result.As<Result<List<DashboardModel>>>();
        // Assert
        resultResponse.IsSuccess.Should().BeTrue();
        resultResponse.Data.Should().BeEmpty();  // Should return an empty list
    }
}
