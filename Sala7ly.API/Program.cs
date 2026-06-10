using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Sala7ly.API.Models;
using Sala7ly.BLL.Common;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.BLL.Services.Implementation;
using Sala7ly.DAL.Common;
using Sala7ly.DAL.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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

builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer(builder.Configuration);
builder.Services.AddScoped<IFilePathProvider, WebHostEnvironmentPathProvider>();
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


    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sala7ly API v1"));


app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();