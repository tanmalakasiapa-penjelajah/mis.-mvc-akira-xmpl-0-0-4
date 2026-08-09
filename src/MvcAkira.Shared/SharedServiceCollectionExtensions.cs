using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MvcAkira.Shared.Data;
using MvcAkira.Shared.Security;
using MvcAkira.Shared.Services;

namespace MvcAkira.Shared;

public static class SharedServiceCollectionExtensions
{
    public static IServiceCollection AddAkiraShared(
        this IServiceCollection services, IConfiguration configuration)
    {
        var fileName = configuration.GetConnectionString("Akira")
                       ?? "akira-0-0-4.db";
        var abs = DbPath.Absolute(fileName);
        services.AddDbContext<AkiraDbContext>(o =>
            o.UseSqlite(new SqliteConnectionStringBuilder { DataSource = abs }.ToString()));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserAccessor, HttpCurrentUserAccessor>();
        services.AddScoped<OtoritasService>();
        services.AddScoped<LogService>();
        services.AddScoped<BacaService>();
        services.AddScoped<TulisService>();

        return services;
    }

    /// <summary>
    /// Registrasi JWT Bearer standar (dipakai Auth, Read, Write agar key tidak
    /// tersebar). Key diambil dari konfigurasi <c>Jwt:Key</c>; untuk lingkungan
    /// produksi wajib melewati env var <c>AKIRA_JWT_KEY</c> (dengan fallback
    /// ke appsettings).
    /// </summary>
    public static IServiceCollection AddAkiraJwt(
        this IServiceCollection services, IConfiguration configuration)
    {
        var jwtCfg = configuration.GetSection("Jwt").Get<JwtConfig>() ?? new JwtConfig();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtCfg.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtCfg.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = jwtCfg.SigningKey(),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                };
            });
        services.AddAuthorization();
        return services;
    }
}