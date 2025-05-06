using Librow.API.Filters;
using Librow.Application.Common.Enums;
using Librow.Application.Models;
using Librow.Application.Models.Requests;
using Librow.Application.Services;
using Librow.Application.Services.Implement;
using Microsoft.AspNetCore.Mvc;

namespace Librow.API.Controllers;
[Route("api/users")]
[ApiController]
public class UserController : ApiControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        var res = await _userService.Login(loginRequest);
        return ApiResponse(res);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        var res = await _userService.Register(registerRequest);
        return ApiResponse(res);
    }

    [RoleAuthorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest logoutRequest)
    {
        var res = await _userService.Logout(logoutRequest);
        return ApiResponse(res);
    }


    [HttpPost("extend-session")]
    public async Task<IActionResult> ExtendSession([FromBody] ExtendSessionRequest extendSessionRequest)
    {
        var res = await _userService.ExtendSession(extendSessionRequest);
        return ApiResponse(res);
    }

    [RoleAuthorize]
    [HttpGet("get-current-context")]
    public async Task<IActionResult> GetCurrentUserContext()
    {
        var res = await _userService.GetCurrentUserContext();
        return ApiResponse(res);
    }

    [RoleAuthorize(AuthRole.Admin)]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] FilterRequest filter)
    {
        var res = await _userService.GetAll(filter);
        return ApiResponse(res);
    }

    [RoleAuthorize(AuthRole.Admin)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var res = await _userService.GetById(id);
        return ApiResponse(res);
    }

    [RoleAuthorize(AuthRole.Admin)]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] RegisterRequest registerRequest)
    {
        var res = await _userService.Add(registerRequest);
        return ApiResponse(res);
    }

    [RoleAuthorize(AuthRole.Admin)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserUpdateRequest updatedUser)
    {
        var res = await _userService.Update(id, updatedUser);
        return ApiResponse(res);
    }

    [RoleAuthorize(AuthRole.Admin)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var res = await _userService.Delete(id);
        return ApiResponse(res);
    }


    [RoleAuthorize(AuthRole.Admin)]
    [HttpGet("get-activity-logs")]
    public async Task<IActionResult> GetActivityLog([FromQuery] FilterRequest filter)
    {
        var res = await _userService.GetActivityLog(filter);
        return ApiResponse(res);
    }
}
