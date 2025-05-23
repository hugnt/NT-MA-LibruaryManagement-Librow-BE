﻿using Librow.API.Filters;
using Librow.Application.Common.Enums;
using Librow.Application.Models;
using Librow.Application.Models.Requests;
using Librow.Application.Services;
using Librow.Application.Services.Implement;
using Librow.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Librow.API.Controllers;

[Route("api/book-borrowing-requests")]
[ApiController]

[RoleAuthorize]
public class BookBorrowingRequestController : ApiControllerBase
{
    private readonly IBookBorrowingRequestService _bookBorrowingRequestService;

    public BookBorrowingRequestController(IBookBorrowingRequestService bookBorrowingReuestService)
    {
        _bookBorrowingRequestService = bookBorrowingReuestService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] BorrowingRequestFilter filter)
    {
        var res = await _bookBorrowingRequestService.GetAll(filter);
        return ApiResponse(res);
    }

    [HttpGet("user-request-info")]
    public async Task<IActionResult> GetUserRequestInfo([FromQuery] RequestFilter filter)
    {
        var res = await _bookBorrowingRequestService.GetUserRequestInfo(filter);
        return ApiResponse(res);
    }

    [HttpGet("all-borrowing-books")]
    public async Task<IActionResult> GetAllBorrowingBooks([FromQuery] BorrowingRequestFilter filter)
    {
        var res = await _bookBorrowingRequestService.GetAllBorrowingBooks(filter);
        return ApiResponse(res);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var res = await _bookBorrowingRequestService.GetById(id);
        return ApiResponse(res);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] BorrowingRequest newBookBorrowingRequest)
    {
        var res = await _bookBorrowingRequestService.Add(newBookBorrowingRequest);
        return ApiResponse(res);
    }

    [RoleAuthorize(AuthRole.Admin)]
    [HttpPatch("update-status/{id}")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest updatedStatusRequest)
    {
        var res = await _bookBorrowingRequestService.UpdateStatus(id, updatedStatusRequest);
        return ApiResponse(res);
    }

    [HttpPatch("details/{id}/extend-due-date")]
    public async Task<IActionResult> UpdateExtendedDueDate(Guid id, [FromBody] ExtendBorrowingRequest extendedDueDate)
    {
        var res = await _bookBorrowingRequestService.UpdateExtendedDueDate(id, extendedDueDate);
        return ApiResponse(res);
    }


}
