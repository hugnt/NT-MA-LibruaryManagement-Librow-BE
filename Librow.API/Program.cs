using Coravel;
using Librow.API.Filters;
using Librow.API.Middlewares;
using Librow.API.OpenApi;
using Librow.Application;
using Librow.Application.BackgroundJobs;
using Librow.Infrastructure;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
//Exception handlers
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers(config => config.Filters.Add(typeof(ValidateModelAttribute)));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(SwaggerGenOptionsConfig.ConfigureSwaggerGenOptions);

// Add db & repositoriy settings to the container.
builder.Services.AddDataAccess(builder.Configuration);

// Add services to the container.
builder.Services.AddApplication(builder.Configuration);

var app = builder.Build();
app.UseExceptionHandler();
app.UseMiddleware<JwtMiddleware>();

//add backgroung jobs
app.Services.UseScheduler(scheduler =>
{
    scheduler.Schedule<CheckOverdueBorrowedBooksJob>().DailyAtHour(8);
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// This applies any pending migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate(); 
}

//CORS Configuration
app.UseCors(corsPolicyBuilder => corsPolicyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
