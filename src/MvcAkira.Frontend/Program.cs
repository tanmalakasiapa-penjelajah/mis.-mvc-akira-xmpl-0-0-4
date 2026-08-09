using Scalar.AspNetCore;
using Serilog;
using MvcAkira.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/frontend-.log", rollingInterval: RollingInterval.Day));

builder.Services.AddOpenApi();
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
app.UseRouting();
app.UseCors("mycors");
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapFallbackToFile("index.html");
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }