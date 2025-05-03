using Librow.API.Filters;
using Librow.Application.Common.Enums;
using Librow.Application.Models;
using Librow.Application.Models.Requests;
using Librow.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Librow.API.Controllers;

[Route("api/book-ratings")]
[ApiController]

[RoleAuthorize]
public class BookRatingController : ApiControllerBase
{
    private readonly IBookRatingService _bookRatingService;

    public BookRatingController(IBookRatingService bookRatingService)
    {
        _bookRatingService = bookRatingService;
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetByBookId(Guid id)
    {
        var res = await _bookRatingService.GetByBookId(id);
        return ApiResponse(res);
    }

    [HttpGet("{id}/user-rights")]
    public async Task<IActionResult> GetUserRight(Guid id)
    {
        var res = await _bookRatingService.GetUserRight(id);
        return ApiResponse(res);
    }


    [HttpPost]
    public async Task<IActionResult> Add([FromBody] BookRatingRequest newBookRating)
    {
        var res = await _bookRatingService.Add(newBookRating);
        return ApiResponse(res);
    }

}
