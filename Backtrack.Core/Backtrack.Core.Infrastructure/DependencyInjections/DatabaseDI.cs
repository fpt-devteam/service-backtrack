using Backtrack.Core.Infrastructure.Configurations;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Backtrack.Core.Infrastructure.DependencyInjections
{
    public static class DatabaseDI
    {
        public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DatabaseSettings>(configuration.GetSection("ConnectionStrings"));

            var connString = configuration.GetConnectionString("DefaultConnection");

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connString);
            dataSourceBuilder.UseVector(); 
            var dataSource = dataSourceBuilder.Build();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connString, o =>
                {
                    o.UseNetTopologySuite();
                    o.UseVector();
                }));
        }
    }
}