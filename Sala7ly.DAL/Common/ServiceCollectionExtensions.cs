using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sala7ly.DAL.DataBase;

namespace Sala7ly.DAL.Common
{

    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddDataAccessLayer(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("ServerConnection")));
           
       
            return services;
        }
    }
}
