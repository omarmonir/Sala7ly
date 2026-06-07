using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sala7ly.DAL.DataBase;
using Sala7ly.DAL.Repositories.Abstraction;
using Sala7ly.DAL.Repositories.Implementation;

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
            AppContext.SetSwitch("Switch.System.Net.Mail.MailMessage.AllowUnicode", true);
            services.AddScoped<ITechnicianProfileRepository, TechnicianProfileRepository>();
            services.AddScoped<ITechnicianPortfolioRepository, TechnicianPortfolioRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            return services;
        }
    }
}
