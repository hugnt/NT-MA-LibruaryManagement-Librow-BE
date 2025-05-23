﻿using Librow.Application.Services;
using Librow.Application.Services.Implement;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Librow.Application.Common.Security.Token;
using Librow.Application.Common.Email;
using Librow.Application.BackgroundJobs;
using Coravel;

namespace Librow.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        //Token & Authentication
        services.Configure<TokenSettings>(configuration.GetSection("TokenSettings"));
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = TokenProvider.TokenValidationParameters;
        });
        services.AddSingleton<ITokenService, TokenService>();

        //Email
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddTransient<IEmailService, EmailService>();

        //Access context
        services.AddHttpContextAccessor();

        //Fluent validation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        //Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBookCategoryService, BookCategoryService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IBookBorrowingRequestService, BookBorrowingRequestService>();
        services.AddScoped<IBookRatingService, BookRatingService>();
        services.AddScoped<IDashboardService, DashboardService>();

        //Background Job
        services.AddScheduler();
        services.AddTransient<CheckOverdueBorrowedBooksJob>();

        return services;
    }
}