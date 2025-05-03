using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Models;

public class Result
{
    public bool IsSuccess { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public string Message { get; set; } = "";
    public List<string> Errors { get; set; } = [];

    public static Result Success(HttpStatusCode statusCode, string message) => new() { IsSuccess = true, StatusCode = statusCode, Message = message };
    public static Result SuccessWithMessage(string message) => new() { IsSuccess = true, StatusCode = HttpStatusCode.OK, Message = message };
    public static Result SuccessNoContent() => new() { IsSuccess = true, StatusCode = HttpStatusCode.NoContent};

    public static Result Error(HttpStatusCode statusCode, string message) => new() { IsSuccess = false, StatusCode = statusCode, Errors = [message] };
    public static Result ErrorNotFound(string message) => new() { IsSuccess = false, StatusCode = HttpStatusCode.NotFound, Errors = [message] };
    public static Result ErrorWithMessage(string message) => new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest, Errors = [message] };
    public static Result ErrorList(HttpStatusCode statusCode, List<string> errors) => new() { IsSuccess = false, StatusCode = statusCode,  Errors = errors };
    public static Result ErrorValidation(ValidationResult validationResult) => new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest, Errors = validationResult.Errors.Select(x => x.ErrorMessage).ToList() };

}

public class Result<T> : Result
{
    public T Data { get; set; }
    public static Result<T> Success(HttpStatusCode statusCode, T body, string message) => new() { IsSuccess = true, StatusCode = statusCode, Message = message };
    public static Result<T> SuccessWithBody(T body) => new() { IsSuccess = true, StatusCode = HttpStatusCode.OK, Message = "Ok", Data = body };
    public static Result<T> ErrorWithBody(T body) => new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest, Message = "Error", Data = body };

}
