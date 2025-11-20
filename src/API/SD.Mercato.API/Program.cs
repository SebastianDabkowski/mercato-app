using SD.Mercato.Users;
using SD.Mercato.SellerPanel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Users module (authentication and authorization)
builder.Services.AddUsersModule(builder.Configuration);

// Add SellerPanel module (store management)
builder.Services.AddSellerPanelModule(builder.Configuration);

// Add CORS
// TODO: Restrict CORS to specific origins (frontend URL) before production deployment
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetService<ILogger<Program>>();
    try
    {
        await UsersModuleExtensions.SeedRolesAsync(services);
    }
    catch (Exception ex)
    {
        if (logger != null)
        {
            logger.LogError(ex, "An error occurred while seeding roles.");
        }
        else
        {
            Console.Error.WriteLine($"An error occurred while seeding roles: {ex}");
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
