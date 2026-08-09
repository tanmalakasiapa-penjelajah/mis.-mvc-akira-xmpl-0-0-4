using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MvcAkira.Shared.Entities;
using MvcAkira.Shared.Security;

namespace MvcAkira.Auth.Services;

public interface IJwtService
{
    string Generate(MejaPengguna pengguna, string? nama, string? tokoCode, string? tokoName, string? jabatan, bool isSuperuser);
}

public class JwtService : IJwtService
{
    private readonly JwtConfig _cfg;

    public JwtService(IOptions<JwtConfig> cfg) => _cfg = cfg.Value;

    public string Generate(MejaPengguna pengguna, string? nama, string? tokoCode, string? tokoName, string? jabatan, bool isSuperuser)
    {
        var claims = new List<Claim>
        {
            new(JwtClaims.PenggunaCode, pengguna.PenggunaCode),
            new(JwtClaims.Email, pengguna.PenggunaEmail),
            new(JwtClaims.Nama, nama ?? ""),
            new(JwtClaims.TokoCode, tokoCode ?? ""),
            new(JwtClaims.TokoName, tokoName ?? ""),
            new(JwtClaims.Jabatan, jabatan ?? ""),
            new(JwtClaims.IsSuperuser, isSuperuser.ToString().ToLowerInvariant()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = _cfg.SigningKey();
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _cfg.Issuer,
            audience: _cfg.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_cfg.ExpiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}