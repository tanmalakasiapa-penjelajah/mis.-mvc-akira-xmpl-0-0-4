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
    .WriteTo.File("logs/write-.log", rollingInterval: RollingInterval.Day));

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddAkiraShared(builder.Configuration);
builder.Services.AddAkiraJwt(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddCors(o => o.AddPolicy("mycors", p => p
    .AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseSerilogRequestLogging();
app.UseMiddleware<MvcAkira.Shared.Services.ApiExceptionMiddleware>();
app.UseRouting();
app.UseCors("mycors");
app.UseAuthentication();
app.UseAuthorization();

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