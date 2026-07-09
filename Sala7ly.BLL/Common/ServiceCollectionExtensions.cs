using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.BLL.Services.Implementation;
using Sala7ly.DAL.DataBase;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Repositories.Abstraction;
using Sala7ly.DAL.Repositories.Implementation;
using System.Security.Claims;
using System.Text;
using static Sala7ly.BLL.Services.Implementation.TechnicianMatchingService;

namespace Sala7ly.BLL.Common
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddJwtAuthentication(configuration);
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAiSupportService, AiSupportService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IServiceCategoryService, ServiceCategoryService>();
            services.AddScoped<ITechnicianService, TechnicianService>();
            services.AddScoped<ITechnicianPortfolioService, TechnicianPortfolioService>();
            services.AddScoped<ITechnicianVerificationService, TechnicianVerificationService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IReviewService, ReviewService>();

            // ── AI module ────────────────────────────────────────────────────
            services.AddScoped<IPriceEstimationService, PriceEstimationService>();
            services.AddScoped<IImageAnalysisService, ImageAnalysisService>();
            services.AddScoped<ITechnicianMatchingService, TechnicianMatchingService>();
            services.AddScoped<IEmbeddingService, GeminiEmbeddingService>();
            services.AddScoped<IRequestRefinerService, RequestRefinerService>();
            services.AddScoped<IRequestDispatchService, RequestDispatchService>();

            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IServiceRequestService, ServiceRequestService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IBidService, BidService>();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:5173",
                            "http://localhost:4200",
                            "https://sala7ly.runasp.net",
                            "http://localhost:5752"
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });

                // Development-only policy
                options.AddPolicy("AllowAllDev", policy =>
                {
                    policy
                        .SetIsOriginAllowed(_ => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });
            services.AddHttpContextAccessor();
            return services;
        }

        private static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("Jwt");
            var secretKey = jwtSettings["Key"];

            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ÇÃÅÂÈÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÝÞßáãäåæíìÁÄÆ";
            }
            ).AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(secretKey!)),
                NameClaimType = ClaimTypes.NameIdentifier,
                ClockSkew = TimeSpan.Zero
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];

                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
    (path.StartsWithSegments("/hubs") ||
     path.StartsWithSegments("/chathub") ||
     path.StartsWithSegments("/notificationhub")))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;
                options.Events.OnRedirectToLogin = ctx =>
                {
                    ctx.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = ctx =>
                {
                    ctx.Response.StatusCode = 403;
                    return Task.CompletedTask;
                };
            });

            return services;
        }
    }
}