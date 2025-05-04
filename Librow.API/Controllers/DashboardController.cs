using Librow.API.Filters;
using Librow.Application.Common.Enums;
using Librow.Application.Models;
using Librow.Application.Models.Requests;
using Librow.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Librow.API.Controllers;

[Route("api/dashboard")]
[ApiController]

[RoleAuthorize(AuthRole.Admin)]
public class DashboardController : ApiControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary-info")]
    public async Task<IActionResult> GetSummaryInfo()
    {
        var res = await _dashboardService.GetSummaryInfo();
        return ApiResponse(res);
    }

    [HttpGet("popular-books")]
    public async Task<IActionResult> GetPopularBooks(int top = 5)
    {
        var res = await _dashboardService.GetPopularBooks(top);
        return ApiResponse(res);
    }

    [HttpGet("request-analysis")]
    public async Task<IActionResult> GetRequestAnalysis()
    {
        var res = await _dashboardService.GetRequestAnalysis();
        return ApiResponse(res);
    }



}
