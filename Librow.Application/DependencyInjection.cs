using Librow.Application.Services;
using Librow.Application.Services.Implement;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using System.Text;
using Librow.Application.Common.Security.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Librow.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        //Token & Authentication
        TokenProvider.SetSecretKey(configuration["TokenSettings:SecretKey"]);
        TokenProvider.SetAccessTokenExprirationTime(int.Parse(configuration["TokenSettings:AccessTokenExpirationInMinutes"] ??"-1"));
        TokenProvider.SetRefreshTokenExprirationTime(int.Parse(configuration["TokenSettings:RefreshTokenExpirationInMinutes"] ?? "-1"));
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = TokenProvider.TokenValidationParameters;
        });

        //Access context
        services.AddHttpContextAccessor();

        //Fluent validation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        //Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBookCategoryService, BookCategoryService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IBookBorrowingRequestService, BookBorrowingRequestService>();

        return services;
    }
}