using Backtrack.Core.Infrastructure.Data;
using Backtrack.Core.WebApi.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backtrack.Core.WebApi.Extensions
{
    public static class DatabaseDI
    {
        public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DatabaseSettings>(configuration.GetSection("ConnectionStrings"));

            var connString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connString, o =>
                {
                    o.UseNetTopologySuite();
                    o.UseVector();
                }));
        }
    }
}