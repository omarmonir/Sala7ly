using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Sala7ly.API.Hubs;
using Sala7ly.API.Models;
using Sala7ly.BLL.Common;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.BLL.Services.Implementation;
using Sala7ly.DAL.Common;
using Sala7ly.DAL.Entities;
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

// IGitHubAiClient now lives in Sala7ly.BLL.Services.Abstraction (was
// incorrectly declared in the DAL, which must never own an AI-provider
// contract), hence no DAL using is needed here anymore.
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

// Named HttpClients used by the AI module. The old "GitHubModels" client
// (with its own separate GitHubModels:Token config key) is gone — every
// LLM call now goes through IGitHubAiClient / AI:GitHub:* instead.
builder.Services.AddHttpClient("ImageDownloader", client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddHttpClient("GeminiEmbeddings", client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
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