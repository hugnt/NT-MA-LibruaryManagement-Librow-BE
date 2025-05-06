using FluentAssertions;
using Librow.Application.Models;
using Librow.Application.Models.Responses;
using Librow.Application.Services.Implement;
using Librow.Application.Tests.MockSetup;
using Librow.Core.Entities;
using Librow.Infrastructure.Repositories.Base;
using Moq;
using System.Linq.Expressions;

namespace Librow.Application.Tests.Services.UserServiceTests;

public class GetActivityLog
{

    private readonly Mock<IRepository<AuditLog>> _mockAuditLogRepository;
    private readonly UserService _userService;
    List<AuditLog> MockAuditlog = MockAuditLogRepositorySetup.ListAuditLogs();
    public GetActivityLog()
    {
        _mockAuditLogRepository = new Mock<IRepository<AuditLog>>();
        _userService = new UserService(null!, null!, null!, null!, null!, null!, _mockAuditLogRepository.Object, null!);
        _mockAuditLogRepository.Setup(r =>
                                r.GetByFilterAsync(
                                    It.IsAny<int?>(),
                                    It.IsAny<int?>(),
                                    It.IsAny<Expression<Func<AuditLog, AuditLogResponse>>>(),
                                    It.IsAny<Expression<Func<AuditLog, bool>>>(),
                                    It.IsAny<CancellationToken>(),
                                    It.IsAny<Expression<Func<AuditLog, object>>[]>()
                                )).ReturnsAsync((int? pageSize, int? pageNumber,
                                                Expression<Func<AuditLog, AuditLogResponse>> selectQuery,
                                                Expression<Func<AuditLog, bool>>? predicate = null,
                                                CancellationToken token = default,
                                                params Expression<Func<AuditLog, object>>[] navigationProperties) =>
                                {
                                    var allData = MockAuditlog.Select(selectQuery.Compile());
                                    var projectedQuery = allData;
                                    if (pageSize.HasValue && pageNumber.HasValue)
                                    {
                                        projectedQuery = projectedQuery.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
                                    }
                                    return (projectedQuery.ToList(), allData.Count());
                                });
    }

    [Fact]
    public async Task GetByFilter_NoFilter_ReturnsAllRecords()
    {
        // Arrange
        var filter = new FilterRequest();

        // Act
        var result = await _userService.GetActivityLog(filter);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Should().BeOfType<FilterResult<List<AuditLogResponse>>>();

        var filterResult = result as FilterResult<List<AuditLogResponse>>;
        filterResult!.Data.Should().HaveCount(MockAuditlog.Count);
        filterResult!.TotalRecords.Should().Be(MockAuditlog.Count);
    }

    [Fact]
    public async Task GetByFilter_WithPagination_ReturnsPagedRecords()
    {
        // Arrange
        var filter = new FilterRequest
        {
            PageSize = 6,
            PageNumber = 2
        };

        // Act
        var result = await _userService.GetActivityLog(filter);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Should().BeOfType<FilterResult<List<AuditLogResponse>>>();

        var filterResult = result as FilterResult<List<AuditLogResponse>>;
        filterResult!.Data.Should().HaveCount(4);
        filterResult.TotalRecords.Should().Be(MockAuditlog.Count); 
    }


}
