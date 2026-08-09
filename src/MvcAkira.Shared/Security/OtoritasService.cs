using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MvcAkira.Shared.Data;
using MvcAkira.Shared.Entities;
using MvcAkira.Shared.Enums;

namespace MvcAkira.Shared.Security;

/// <summary>Identitas user aktif dari JWT claims (dibaca via HttpContext).</summary>
public class CurrentUser
{
    public string PenggunaCode { get; set; } = default!;
    public string? Email { get; set; }
    public string? Nama { get; set; }
    public string? TokoCode { get; set; }
    public string? TokoName { get; set; }
    public string? Jabatan { get; set; }
    public bool IsSuperuser { get; set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(PenggunaCode);
}

public interface ICurrentUserAccessor
{
    CurrentUser? User { get; }
}

public class HttpCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _http;

    public HttpCurrentUserAccessor(IHttpContextAccessor http) => _http = http;

    public CurrentUser? User
    {
        get
        {
            var p = _http.HttpContext?.User;
            if (p is null || p.Identity?.IsAuthenticated != true) return null;

            return new CurrentUser
            {
                PenggunaCode = p.FindFirst(JwtClaims.PenggunaCode)?.Value ?? string.Empty,
                Email = p.FindFirst(JwtClaims.Email)?.Value,
                Nama = p.FindFirst(JwtClaims.Nama)?.Value,
                TokoCode = p.FindFirst(JwtClaims.TokoCode)?.Value,
                TokoName = p.FindFirst(JwtClaims.TokoName)?.Value,
                Jabatan = p.FindFirst(JwtClaims.Jabatan)?.Value,
                IsSuperuser = bool.TryParse(p.FindFirst(JwtClaims.IsSuperuser)?.Value, out var s) && s,
            };
        }
    }
}

/// <summary>
/// Memeriksa hak akses per target + isolasi data per toko (plan-0-0-1 bagian H).
/// Pemeriksaan lewat target_name (misal "meja_toko"); di-resolve ke TargetCode.
/// </summary>
public class OtoritasService
{
    private readonly AkiraDbContext _db;
    private readonly ICurrentUserAccessor _current;

    public OtoritasService(AkiraDbContext db, ICurrentUserAccessor current)
    {
        _db = db;
        _current = current;
    }

    private CurrentUser? User => _current.User;

    public Task<bool> IsSuperuserAsync(CancellationToken ct = default)
        => Task.FromResult(User is not null && User.IsAuthenticated && User.IsSuperuser);

    /// <summary>Toko milik user biasa (dari meja_biodata); superuser -> null.</summary>
    public async Task<string?> UserTokoCodeAsync(CancellationToken ct = default)
    {
        if (User is null || !User.IsAuthenticated) return null;
        if (User.IsSuperuser) return null;
        var toko = await _db.MejaBiodata
            .Where(b => b.PenggunaCode == User.PenggunaCode && b.BiodataSoftdeleted == 0)
            .Select(b => (string?)b.TokoCode)
            .FirstOrDefaultAsync(ct);
        return string.IsNullOrEmpty(toko) ? null : toko;
    }

    /// <summary>Akses record ber-toko jika pemilik == toko miliknya (atau superuser).</summary>
    public async Task<bool> PunyaAksesDataTokoAsync(string tokoCodeRecord, CancellationToken ct = default)
    {
        if (User is null || !User.IsAuthenticated) return false;
        if (User.IsSuperuser) return true;
        var userToko = await UserTokoCodeAsync(ct);
        return !string.IsNullOrEmpty(userToko) && userToko == tokoCodeRecord;
    }

    /// <summary>Akses data pribadi: superuser atau record pengguna_code miliknya.</summary>
    public bool PunyaAksesDataPribadi(string penggunaCodeRecord)
        => User is not null && User.IsAuthenticated &&
           (User.IsSuperuser || User.PenggunaCode == penggunaCodeRecord);

    public async Task<bool> BolehBacaAsync(string targetName, CancellationToken ct = default)
    {
        if (User is null || !User.IsAuthenticated) return false;
        if (User.IsSuperuser) return true;
        return await HakFlagAsync(targetName, HakAksi.Read, ct);
    }

    /// <summary>Superuser selalu boleh; master sensitif hanya superuser; selain itu cek flag.</summary>
    public async Task<(bool Ok, string? Pesan)> BolehTulisAsync(
        string targetName, string aksi, CancellationToken ct = default)
    {
        if (User is null || !User.IsAuthenticated) return (false, "autentikasi diperlukan");
        if (User.IsSuperuser) return (true, null);
        if (IsMasterSensitif(targetName))
            return (false, "hanya superuser yang dapat mengubah data master ini");

        var ok = await HakFlagAsync(targetName, aksi, ct);
        return ok ? (true, null) : (false, "tidak memiliki izin untuk aksi ini");
    }

    public async Task<bool> BolehLoginAsync(string? penggunaCode, CancellationToken ct = default)
    {
        if (User is not null && User.IsAuthenticated && User.IsSuperuser) return true;
        if (string.IsNullOrEmpty(penggunaCode)) return false;
        return await HakLoginFlagAsync(penggunaCode, ct);
    }

    private async Task<string?> ResolveTargetCodeAsync(string targetName, CancellationToken ct)
        => await _db.MejaTarget
            .Where(t => t.TargetName == targetName && t.TargetSoftdeleted == 0)
            .Select(t => (string?)t.TargetCode)
            .FirstOrDefaultAsync(ct);

    private async Task<bool> HakFlagAsync(string targetName, string aksi, CancellationToken ct)
    {
        string kolom = aksi switch
        {
            HakAksi.Read => nameof(MejaHakakses.HakaksesRead),
            HakAksi.Create => nameof(MejaHakakses.HakaksesCreate),
            HakAksi.Update => nameof(MejaHakakses.HakaksesUpdate),
            HakAksi.Delete => nameof(MejaHakakses.HakaksesDelete),
            _ => string.Empty,
        };
        if (string.IsNullOrEmpty(kolom) || User is null) return false;

        var targetCode = await ResolveTargetCodeAsync(targetName, ct);
        if (string.IsNullOrEmpty(targetCode)) return false;

        var row = await _db.MejaHakakses
            .FirstOrDefaultAsync(
                h => h.PenggunaCode == User.PenggunaCode
                  && h.TargetCode == targetCode
                  && h.HakaksesSoftdeleted == 0, ct);
        if (row is null) return false;
        return kolom switch
        {
            nameof(MejaHakakses.HakaksesRead) => row.HakaksesRead == 1,
            nameof(MejaHakakses.HakaksesCreate) => row.HakaksesCreate == 1,
            nameof(MejaHakakses.HakaksesUpdate) => row.HakaksesUpdate == 1,
            nameof(MejaHakakses.HakaksesDelete) => row.HakaksesDelete == 1,
            _ => false,
        };
    }

    private async Task<bool> HakLoginFlagAsync(string penggunaCode, CancellationToken ct)
    {
        var targetCode = await ResolveTargetCodeAsync(CoreTables.Pengguna, ct);
        if (string.IsNullOrEmpty(targetCode)) return false;
        return await _db.MejaHakakses.AnyAsync(
            h => h.PenggunaCode == penggunaCode
              && h.TargetCode == targetCode
              && h.HakaksesLogin == 1
              && h.HakaksesSoftdeleted == 0, ct);
    }

    public static bool IsMasterSensitif(string targetName)
        => targetName is CoreTables.Toko or CoreTables.Jabatan or CoreTables.Target;
}

public static class CoreTables
{
    public const string Toko = "meja_toko";
    public const string Pengguna = "meja_pengguna";
    public const string Biodata = "meja_biodata";
    public const string Jabatan = "meja_jabatan";
    public const string Target = "meja_target";
    public const string Hakakses = "meja_hakakses";
    public const string Keuangan = "meja_keuangan";
    public const string Log = "meja_log";
}