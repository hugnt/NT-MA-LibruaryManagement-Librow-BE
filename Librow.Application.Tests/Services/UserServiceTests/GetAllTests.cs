using FluentAssertions;
using Librow.Application.Models;
using Librow.Application.Models.Responses;
using Librow.Application.Services.Implement;
using Librow.Application.Tests.MockSetup;
using Librow.Core.Entities;
using Librow.Infrastructure.Repositories;
using Librow.Infrastructure.Repositories.Base;
using Moq;
using Org.BouncyCastle.Asn1.Pkcs;
using System.Linq.Expressions;

namespace Librow.Application.Tests.Services.UserServiceTests;

public class GetAllTests
{

    private readonly Mock<IRepository<User>> _mockUserRepository;
    private readonly UserService _bookCategoryService;
    List<User> MockUsers = MockUserRepositorySetup.ListUsers();
    public GetAllTests()
    {
        _mockUserRepository = new Mock<IRepository<User>>();
        _bookCategoryService = new UserService(_mockUserRepository.Object, null!, null!, null!, null!, null!, null!, null!);
        _mockUserRepository.Setup(r =>
                                r.GetByFilterAsync(
                                    It.IsAny<int?>(),
                                    It.IsAny<int?>(),
                                    It.IsAny<Expression<Func<User, UserResponse>>>(),
                                    It.IsAny<Expression<Func<User, bool>>>(),
                                    It.IsAny<CancellationToken>(),
                                    It.IsAny<Expression<Func<User, object>>[]>()
                                )).ReturnsAsync((int? pageSize, int? pageNumber,
                                                Expression<Func<User, UserResponse>> selectQuery,
                                                Expression<Func<User, bool>>? predicate = null,
                                                CancellationToken token = default,
                                                params Expression<Func<User, object>>[] navigationProperties) =>
                                {
                                    var allData = MockUsers.Select(selectQuery.Compile());
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
        var result = await _bookCategoryService.GetAll(filter);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Should().BeOfType<FilterResult<List<UserResponse>>>();

        var filterResult = result as FilterResult<List<UserResponse>>;
        filterResult!.Data.Should().HaveCount(MockUsers.Count);
        filterResult!.TotalRecords.Should().Be(MockUsers.Count);
    }

    [Fact]
    public async Task GetByFilter_WithPagination_ReturnsPagedRecords()
    {
        // Arrange
        var filter = new FilterRequest
        {
            PageSize = 2,
            PageNumber = 1
        };

        // Act
        var result = await _bookCategoryService.GetAll(filter);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Should().BeOfType<FilterResult<List<UserResponse>>>();

        var filterResult = result as FilterResult<List<UserResponse>>;
        filterResult!.Data.Should().HaveCount(2);
        filterResult.TotalRecords.Should().Be(MockUsers.Count); 
    }


}
