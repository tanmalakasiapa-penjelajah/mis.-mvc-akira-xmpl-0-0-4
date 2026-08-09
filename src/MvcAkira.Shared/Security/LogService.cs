using MvcAkira.Shared.Data;
using MvcAkira.Shared.Entities;
using MvcAkira.Shared.Enums;
using MvcAkira.Shared.Security;

namespace MvcAkira.Shared.Security;

/// <summary>Mencatat semua aksi tulis & login ke meja_log (plan-0-0-1 C).</summary>
public class LogService
{
    private readonly AkiraDbContext _db;
    private readonly ICurrentUserAccessor _current;

    public LogService(AkiraDbContext db, ICurrentUserAccessor current)
    {
        _db = db;
        _current = current;
    }

    private string Pelaku() => _current.User?.PenggunaCode ?? "";

    public async Task CatatAsync(
        string aksi, string target, string oldValue, string newValue,
        CancellationToken ct = default)
    {
        var now = DateStamp.Now();
        var log = new MejaLog
        {
            LogCode = CodeGenerator.Next("meja_log"),
            LogPelaku = Pelaku(),
            LogMencatat = aksi,
            LogOldvalue = string.IsNullOrEmpty(oldValue) ? "-" : oldValue,
            LogNewvalue = string.IsNullOrEmpty(newValue) ? "-" : newValue,
            LogTarget = target,
            LogSoftdeleted = 0,
            LogCreatedat = now,
            LogUpdatedat = now,
        };
        _db.MejaLog.Add(log);
        await _db.SaveChangesAsync(ct);
    }

    public static string Target(string tableName, string code)
        => $"meja_{tableName}:{tableName}_code={code}";
}

public static class SharedErrorCodes
{
    public const string BelumLogin = "BELUM_LOGIN";
    public const string TidakAdaHak = "TIDAK_PUNYA_HAK";
    public const string TidakDitemukan = "TIDAK_DITEMUKAN";
    public const string DataTidakLengkap = "DATA_TIDAK_LENGKAP";
    public const string Konflik = "KONFLIK";
    public const string Isolasi= "ISOLASI";
}