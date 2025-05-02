using Librow.API.Filters;
using Librow.Application.Common.Enums;
using Librow.Application.Models;
using Librow.Application.Models.Requests;
using Librow.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Librow.API.Controllers;

[Route("api/book-categories")]
[ApiController]

[RoleAuthorize]
public class BookCategoryController : ApiControllerBase
{
    private readonly IBookCategoryService _bookCategoryService;

    public BookCategoryController(IBookCategoryService bookCategoryService)
    {
        _bookCategoryService = bookCategoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] FilterRequest filter)
    {
        var res = await _bookCategoryService.GetAll(filter);
        return ApiResponse(res);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var res = await _bookCategoryService.GetById(id);
        return ApiResponse(res);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] BookCategoryRequest newBookCategory)
    {
        var res = await _bookCategoryService.Add(newBookCategory);
        return ApiResponse(res);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] BookCategoryRequest updatedBookCategory)
    {
        var res = await _bookCategoryService.Update(id, updatedBookCategory);
        return ApiResponse(res);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var res = await _bookCategoryService.Delete(id);
        return ApiResponse(res);
    }
}
