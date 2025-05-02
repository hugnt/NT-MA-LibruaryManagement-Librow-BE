using Librow.API.Filters;
using Librow.Application.Common.Enums;
using Librow.Application.Models;
using Librow.Application.Models.Requests;
using Librow.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Librow.API.Controllers;

[Route("api/books")]
[ApiController]

[RoleAuthorize]
public class BookController : ApiControllerBase
{
    private readonly IBookService _bookService;

    public BookController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] FilterRequest filter)
    {
        var res = await _bookService.GetAll(filter);
        return ApiResponse(res);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var res = await _bookService.GetById(id);
        return ApiResponse(res);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] BookRequest newBook)
    {
        var res = await _bookService.Add(newBook);
        return ApiResponse(res);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] BookRequest updatedBook)
    {
        var res = await _bookService.Update(id, updatedBook);
        return ApiResponse(res);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var res = await _bookService.Delete(id);
        return ApiResponse(res);
    }
}
