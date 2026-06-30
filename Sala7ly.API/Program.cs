using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Sala7ly.API.Hubs;
using Sala7ly.API.Models;
using Sala7ly.BLL.Common;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.BLL.Services.Implementation;
using Sala7ly.DAL.Common;
using Sala7ly.DAL.Entities;
using Sala7ly.DAL.Repositories.Abstraction;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("bearerAuthorization", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "bearerAuthorization"
                }
            },
            Array.Empty<string>()
        }
    });
});
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton<IGitHubAiClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var useMock = bool.Parse(config["AI:GitHub:UseMock"] ?? "false");
    return useMock
        ? new MockGitHubAiClient()
        : new GitHubAiClient(config);
});
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer(builder.Configuration);
builder.Services.AddScoped<IFilePathProvider, WebHostEnvironmentPathProvider>();
builder.Services.AddHttpClient("GitHubModels", client =>
{
    client.DefaultRequestHeaders.Add("Authorization",
        $"Bearer {builder.Configuration["GitHubModels:Token"]}");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient("GeminiEmbedding", client =>
{
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    foreach (var role in new[] { "Customer", "Technician", "Admin" })
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<User>>();

    const string testEmail = "ma7311590@gmail.com";

    if (await userManager.FindByEmailAsync(testEmail) == null)
    {
        var adminUser = new User
        {
            Name = "Admin",
            UserName = "admin",
            Email = testEmail,
            EmailConfirmed = true,
        };
        adminUser.Activate();

        var result = await userManager.CreateAsync(adminUser, "Omar@1234");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI(c =>
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sala7ly API v1"));

app.UseHttpsRedirection();

// Use more permissive CORS in development, stricter in production
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAllDev");
}
else
{
    app.UseCors("AllowAll");
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/chathub");
app.MapHub<BiddingHub>("/hubs/bidding");
app.MapHub<NotificationHub>("/notificationhub");

await app.RunAsync();