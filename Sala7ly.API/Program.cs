using Microsoft.EntityFrameworkCore;
using Sala7ly.BLL.Common;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.BLL.Services.Implementation;
using Sala7ly.DAL.Common;
using Sala7ly.DAL.DataBase;
using Sala7ly.DAL.Repositories;
using Sala7ly.DAL.Repositories.Interfaces;

namespace Sala7ly.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddScoped<ITechnicianProfileRepository, TechnicianProfileRepository>();
            builder.Services.AddScoped<ITechnicianPortfolioRepository, TechnicianPortfolioRepository>();
            builder.Services.AddScoped<ITechnicianProfileService, TechnicianProfileService>();
            builder.Services.AddScoped<ITechnicianPortfolioService, TechnicianPortfolioService>();
            builder.Services.AddDataAccessLayer(builder.Configuration);

            //builder.Services.AddBusinessLogicLayer();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
