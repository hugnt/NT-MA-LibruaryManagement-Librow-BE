using Librow.Application.Models;
using Librow.Application.Models.Requests;
using Librow.Application.Services;
using Microsoft.AspNetCore.Http;
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

    [HttpPost("extend-session")]
    public async Task<IActionResult> ExtendSession([FromBody] ExtendSessionRequest extendSessionRequest)
    {
        var res = await _userService.ExtendSession(extendSessionRequest);
        return ApiResponse(res);
    }
}
