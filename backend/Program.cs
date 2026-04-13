using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Web;
using Tambouille.Data;
using Tambouille.DTOs;
using Tambouille.Endpoints;
using Tambouille.Services;
using Tambouille.Validators;

var logger = LogManager.Setup().LoadConfigurationFromFile().GetCurrentClassLogger();
logger.Info("Starting Tambouille");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Database
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

    // Auth
    builder.Services.AddScoped<AuthService>();

    var jwtSecret = builder.Configuration["Auth:JwtSecret"]
        ?? throw new InvalidOperationException("Auth:JwtSecret not configured");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });

    builder.Services.AddAuthorization();

    // Validators
    builder.Services.AddScoped<IValidator<CreateMealRequest>, CreateMealRequestValidator>();
    builder.Services.AddScoped<IValidator<UpdateMealRequest>, UpdateMealRequestValidator>();
    builder.Services.AddScoped<IValidator<MoveMealRequest>, MoveMealRequestValidator>();

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins("http://localhost:5173", "https://tambouille.persistor.ovh")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var app = builder.Build();

    // Apply migrations on startup
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }

    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapAuthEndpoints();
    app.MapMealEndpoints();

    logger.Info(@" _____ _   __  __ ___  ___  _   _ ___ _    _    ___ ");
    logger.Info(@" |_   _/_\ |  \/  | _ )/ _ \| | | |_ _| |  | |  | __|");
    logger.Info(@"   | |/ _ \| |\/| | _ \ (_) | |_| || || |__| |__| _| ");
    logger.Info(@"   |_/_/ \_\_|  |_|___/\___/ \___/|___|____|____|___|");
    logger.Info(@"                                                     ");
    // https://patorjk.com/software/taag/#p=display&f=Small&t=TAMBOUILLE

    var hostTask = app.RunAsync();
    var server = app.Services.GetService<IServer>();
    var serverAddressesFeature = server!.Features.Get<IServerAddressesFeature>();
    var address = serverAddressesFeature!.Addresses.First();
    logger.Info($"Listening on: {address}");

    await hostTask;
}
catch (Exception exception)
{
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}
