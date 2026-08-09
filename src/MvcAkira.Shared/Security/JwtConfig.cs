using Microsoft.IdentityModel.Tokens;

namespace MvcAkira.Shared.Security;

public class JwtConfig
{
    public string Issuer { get; set; } = "MvcAkira.Auth";
    public string Audience { get; set; } = "MvcAkira.App";
    public string Key { get; set; } = "dev-sekali-set-akira-0-0-4-change-me-aaaaaaaa";

    /// <summary>Kunci signing; env <c>AKIRA_JWT_KEY</c> dipakai bila tersedia.</summary>
    public SymmetricSecurityKey SigningKey()
    {
        var k = Environment.GetEnvironmentVariable("AKIRA_JWT_KEY") ?? Key;
        return new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(k));
    }

    public int ExpiryMinutes { get; set; } = 120;
}

public class JwtClaims
{
    public const string PenggunaCode = "pengguna_code";
    public const string Email = "email";
    public const string Nama = "nama";
    public const string TokoCode = "toko_code";
    public const string TokoName = "toko_name";
    public const string Jabatan = "jabatan";
    public const string IsSuperuser = "is_superuser";
}