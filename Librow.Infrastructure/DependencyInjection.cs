using Librow.Infrastructure.Repositories;
using Librow.Infrastructure.Repositories.Base;
using Librow.Infrastructure.Repositories.Implement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        //db
        string connectionString = configuration.GetConnectionString("Database")!;

        services.AddDbContextPool<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        }, poolSize: 128);

        //Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IBookCategoryRepository, BookCategoryRepository>();
        services.AddScoped<IBookBorrowingRequestRepository, BookBorrowingRequestRepository>();

        return services;
    }
}
