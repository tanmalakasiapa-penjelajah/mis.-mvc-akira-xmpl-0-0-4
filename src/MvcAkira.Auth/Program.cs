using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using MvcAkira.Shared;
using MvcAkira.Shared.Data;
using MvcAkira.Shared.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/auth-.log", rollingInterval: RollingInterval.Day));

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddAkiraShared(builder.Configuration);

// JWT config
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<MvcAkira.Auth.Services.IJwtService, MvcAkira.Auth.Services.JwtService>();
builder.Services.AddAkiraJwt(builder.Configuration);

// Health
builder.Services.AddHealthChecks();

// Rate limiting
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddFixedWindowLimiter("login", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    o.AddFixedWindowLimiter("default", opt =>
    {
        opt.PermitLimit = 120;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});

// CORS
const string MyCors = "mycors";
builder.Services.AddCors(o => o.AddPolicy(MyCors, p => p
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseSerilogRequestLogging();
app.UseRouting();

app.UseCors(MyCors);
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AkiraDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await db.Database.MigrateAsync();
    await Seeder.SeedAsync(db, logger);
}

app.Run();

public partial class Program { }