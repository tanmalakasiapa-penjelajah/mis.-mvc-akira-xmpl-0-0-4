using Microsoft.EntityFrameworkCore;
using MvcAkira.Shared.Data;
using MvcAkira.Shared.Entities;
using MvcAkira.Shared.Enums;
using MvcAkira.Shared.Security;

namespace MvcAkira.Shared.Services;

public record HasilTulis(bool Ok, string? Kode = null, string? Pesan = null, int Status = 200)
{
    public static HasilTulis Gagal(string pesan, int status = 400, string? kode = null)
        => new(false, kode, pesan, status);
    public static HasilTulis Sukses(string kode) => new(true, kode);
}

/// <summary>Semua aksi tulis (create/update/softdelete/restore/permanent) + log + isolasi.</summary>
public class TulisService
{
    private readonly AkiraDbContext _db;
    private readonly OtoritasService _otoritas;
    private readonly LogService _log;

    public TulisService(AkiraDbContext db, OtoritasService otoritas, LogService log)
    {
        _db = db;
        _otoritas = otoritas;
        _log = log;
    }

    // ---------- TOKO (master sensitif) ----------
    public async Task<HasilTulis> CreateTokoAsync(string name, string address, string email, string phone, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name)) return HasilTulis.Gagal("nama toko wajib diisi");
        var izin = await _otoritas.BolehTulisAsync(CoreTables.Toko, HakAksi.Create, ct);
        if (!izin.Ok) return HasilTulis.Gagal(izin.Pesan ?? "tidak berhak", 403);
        if (await _db.MejaToko.AnyAsync(t => t.TokoName == name && t.TokoSoftdeleted == 0, ct))
            return HasilTulis.Gagal("nama toko sudah ada", 409, "KONFLIK");

        var now = DateStamp.Now();
        var e = new MejaToko
        {
            TokoCode = CodeGenerator.Next("meja_toko"),
            TokoName = name, TokoAddress = address ?? "", TokoEmail = email ?? "", TokoPhone = phone ?? "",
            TokoSoftdeleted = 0, TokoCreatedat = now, TokoUpdatedat = now,
        };
        _db.MejaToko.Add(e);
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Create, LogService.Target(CoreTables.Toko, e.TokoCode), "-", e.TokoName, ct);
        return HasilTulis.Sukses(e.TokoCode);
    }

    public async Task<HasilTulis> UpdateTokoAsync(string code, string? name, string? address, string? email, string? phone, CancellationToken ct)
    {
        var e = await _db.MejaToko.FirstOrDefaultAsync(t => t.TokoCode == code && t.TokoSoftdeleted == 0, ct);
        if (e is null) return HasilTulis.Gagal("toko tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        var izin = await _otoritas.BolehTulisAsync(CoreTables.Toko, HakAksi.Update, ct);
        if (!izin.Ok) return HasilTulis.Gagal(izin.Pesan ?? "tidak berhak", 403);

        var old = e.TokoName;
        e.TokoName = name ?? e.TokoName;
        e.TokoAddress = address ?? e.TokoAddress;
        e.TokoEmail = email ?? e.TokoEmail;
        e.TokoPhone = phone ?? e.TokoPhone;
        e.TokoUpdatedat = DateStamp.Now();
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Update, LogService.Target(CoreTables.Toko, e.TokoCode), old, e.TokoName, ct);
        return HasilTulis.Sukses(e.TokoCode);
    }

    public async Task<HasilTulis> SoftDeleteTokoAsync(string code, CancellationToken ct)
    {
        var e = await _db.MejaToko.FirstOrDefaultAsync(t => t.TokoCode == code && t.TokoSoftdeleted == 0, ct);
        if (e is null) return HasilTulis.Gagal("toko tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        var izin = await _otoritas.BolehTulisAsync(CoreTables.Toko, HakAksi.Delete, ct);
        if (!izin.Ok) return HasilTulis.Gagal(izin.Pesan ?? "tidak berhak", 403);
        if (await _db.MejaBiodata.AnyAsync(b => b.TokoCode == code && b.BiodataSoftdeleted == 0, ct)
            || await _db.MejaKeuangan.AnyAsync(k => k.TokoCode == code && k.KeuanganSoftdeleted == 0, ct))
            return HasilTulis.Gagal("toko masih dipakai data lain, tidak bisa dihapus", 409, "ANTI_ORPHAN");

        e.TokoSoftdeleted = 1; e.TokoUpdatedat = DateStamp.Now();
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Softdelete, LogService.Target(CoreTables.Toko, e.TokoCode), e.TokoName, "deleted", ct);
        return HasilTulis.Sukses(e.TokoCode);
    }

    public async Task<HasilTulis> RestoreTokoAsync(string code, CancellationToken ct)
    {
        var e = await _db.MejaToko.FirstOrDefaultAsync(t => t.TokoCode == code && t.TokoSoftdeleted == 1, ct);
        if (e is null) return HasilTulis.Gagal("toko tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        e.TokoSoftdeleted = 0; e.TokoUpdatedat = DateStamp.Now();
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Restore, LogService.Target(CoreTables.Toko, e.TokoCode), "deleted", e.TokoName, ct);
        return HasilTulis.Sukses(e.TokoCode);
    }

    public async Task<HasilTulis> PermanentTokoAsync(string code, CancellationToken ct)
    {
        var e = await _db.MejaToko.FirstOrDefaultAsync(t => t.TokoCode == code && t.TokoSoftdeleted == 1, ct);
        if (e is null) return HasilTulis.Gagal("toko tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        _db.MejaToko.Remove(e);
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Delete, LogService.Target(CoreTables.Toko, e.TokoCode), e.TokoName, "permanent", ct);
        return HasilTulis.Sukses(code);
    }

    // ---------- JABATAN (master sensitif) ----------
    public async Task<HasilTulis> CreateJabatanAsync(string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name)) return HasilTulis.Gagal("nama jabatan wajib diisi");
        var izin = await _otoritas.BolehTulisAsync(CoreTables.Jabatan, HakAksi.Create, ct);
        if (!izin.Ok) return HasilTulis.Gagal(izin.Pesan ?? "tidak berhak", 403);
        if (await _db.MejaJabatan.AnyAsync(j => j.JabatanName == name && j.JabatanSoftdeleted == 0, ct))
            return HasilTulis.Gagal("jabatan sudah ada", 409, "KONFLIK");
        var now = DateStamp.Now();
        var e = new MejaJabatan
        { JabatanCode = CodeGenerator.Next("meja_jabatan"), JabatanName = name,
          JabatanSoftdeleted = 0, JabatanCreatedat = now, JabatanUpdatedat = now };
        _db.MejaJabatan.Add(e);
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Create, LogService.Target(CoreTables.Jabatan, e.JabatanCode), "-", e.JabatanName, ct);
        return HasilTulis.Sukses(e.JabatanCode);
    }

    public async Task<HasilTulis> UpdateJabatanAsync(string code, string name, CancellationToken ct)
    {
        var e = await _db.MejaJabatan.FirstOrDefaultAsync(j => j.JabatanCode == code && j.JabatanSoftdeleted == 0, ct);
        if (e is null) return HasilTulis.Gagal("jabatan tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        var izin = await _otoritas.BolehTulisAsync(CoreTables.Jabatan, HakAksi.Update, ct);
        if (!izin.Ok) return HasilTulis.Gagal(izin.Pesan ?? "tidak berhak", 403);
        if (string.IsNullOrWhiteSpace(name)) return HasilTulis.Gagal("nama jabatan wajib diisi");
        if (await _db.MejaJabatan.AnyAsync(j => j.JabatanName == name && j.JabatanCode != code && j.JabatanSoftdeleted == 0, ct))
            return HasilTulis.Gagal("jabatan sudah ada", 409, "KONFLIK");
        var old = e.JabatanName;
        e.JabatanName = name; e.JabatanUpdatedat = DateStamp.Now();
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Update, LogService.Target(CoreTables.Jabatan, e.JabatanCode), old, e.JabatanName, ct);
        return HasilTulis.Sukses(e.JabatanCode);
    }

    public async Task<HasilTulis> SoftDeleteJabatanAsync(string code, CancellationToken ct)
    {
        var e = await _db.MejaJabatan.FirstOrDefaultAsync(j => j.JabatanCode == code && j.JabatanSoftdeleted == 0, ct);
        if (e is null) return HasilTulis.Gagal("jabatan tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        var izin = await _otoritas.BolehTulisAsync(CoreTables.Jabatan, HakAksi.Delete, ct);
        if (!izin.Ok) return HasilTulis.Gagal(izin.Pesan ?? "tidak berhak", 403);
        if (await _db.MejaBiodata.AnyAsync(b => b.JabatanCode == code && b.BiodataSoftdeleted == 0, ct))
            return HasilTulis.Gagal("jabatan masih dipakai biodata", 409, "ANTI_ORPHAN");
        e.JabatanSoftdeleted = 1; e.JabatanUpdatedat = DateStamp.Now();
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Softdelete, LogService.Target(CoreTables.Jabatan, e.JabatanCode), e.JabatanName, "deleted", ct);
        return HasilTulis.Sukses(e.JabatanCode);
    }

    public async Task<HasilTulis> RestoreJabatanAsync(string code, CancellationToken ct)
    {
        var e = await _db.MejaJabatan.FirstOrDefaultAsync(j => j.JabatanCode == code && j.JabatanSoftdeleted == 1, ct);
        if (e is null) return HasilTulis.Gagal("jabatan tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        e.JabatanSoftdeleted = 0; e.JabatanUpdatedat = DateStamp.Now();
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Restore, LogService.Target(CoreTables.Jabatan, e.JabatanCode), "deleted", e.JabatanName, ct);
        return HasilTulis.Sukses(e.JabatanCode);
    }

    public async Task<HasilTulis> PermanentJabatanAsync(string code, CancellationToken ct)
    {
        var e = await _db.MejaJabatan.FirstOrDefaultAsync(j => j.JabatanCode == code && j.JabatanSoftdeleted == 1, ct);
        if (e is null) return HasilTulis.Gagal("jabatan tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        _db.MejaJabatan.Remove(e);
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Delete, LogService.Target(CoreTables.Jabatan, e.JabatanCode), e.JabatanName, "permanent", ct);
        return HasilTulis.Sukses(code);
    }

    // ---------- TARGET (master sensitif) ----------
    public async Task<HasilTulis> CreateTargetAsync(string name, string keterangan, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name)) return HasilTulis.Gagal("nama target wajib diisi");
        var izin = await _otoritas.BolehTulisAsync(CoreTables.Target, HakAksi.Create, ct);
        if (!izin.Ok) return HasilTulis.Gagal(izin.Pesan ?? "tidak berhak", 403);
        if (await _db.MejaTarget.AnyAsync(t => t.TargetName == name && t.TargetSoftdeleted == 0, ct))
            return HasilTulis.Gagal("target sudah ada", 409, "KONFLIK");
        var now = DateStamp.Now();
        var e = new MejaTarget
        { TargetCode = CodeGenerator.Next("meja_target"), TargetName = name, TargetKeterangan = keterangan ?? "",
          TargetSoftdeleted = 0, TargetCreatedat = now, TargetUpdatedat = now };
        _db.MejaTarget.Add(e);
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Create, LogService.Target(CoreTables.Target, e.TargetCode), "-", e.TargetName, ct);
        return HasilTulis.Sukses(e.TargetCode);
    }

    public async Task<HasilTulis> UpdateTargetAsync(string code, string name, string keterangan, CancellationToken ct)
    {
        var e = await _db.MejaTarget.FirstOrDefaultAsync(t => t.TargetCode == code && t.TargetSoftdeleted == 0, ct);
        if (e is null) return HasilTulis.Gagal("target tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        var izin = await _otoritas.BolehTulisAsync(CoreTables.Target, HakAksi.Update, ct);
        if (!izin.Ok) return HasilTulis.Gagal(izin.Pesan ?? "tidak berhak", 403);
        if (string.IsNullOrWhiteSpace(name)) return HasilTulis.Gagal("nama target wajib diisi");
        if (await _db.MejaTarget.AnyAsync(t => t.TargetName == name && t.TargetCode != code && t.TargetSoftdeleted == 0, ct))
            return HasilTulis.Gagal("target sudah ada", 409, "KONFLIK");
        var old = e.TargetName;
        e.TargetName = name; e.TargetKeterangan = keterangan ?? e.TargetKeterangan;
        e.TargetUpdatedat = DateStamp.Now();
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Update, LogService.Target(CoreTables.Target, e.TargetCode), old, e.TargetName, ct);
        return HasilTulis.Sukses(e.TargetCode);
    }

    public async Task<HasilTulis> SoftDeleteTargetAsync(string code, CancellationToken ct)
    {
        var e = await _db.MejaTarget.FirstOrDefaultAsync(t => t.TargetCode == code && t.TargetSoftdeleted == 0, ct);
        if (e is null) return HasilTulis.Gagal("target tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        var izin = await _otoritas.BolehTulisAsync(CoreTables.Target, HakAksi.Delete, ct);
        if (!izin.Ok) return HasilTulis.Gagal(izin.Pesan ?? "tidak berhak", 403);
        if (await _db.MejaHakakses.AnyAsync(h => h.TargetCode == code && h.HakaksesSoftdeleted == 0, ct))
            return HasilTulis.Gagal("target masih dipakai hak akses", 409, "ANTI_ORPHAN");
        e.TargetSoftdeleted = 1; e.TargetUpdatedat = DateStamp.Now();
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Softdelete, LogService.Target(CoreTables.Target, e.TargetCode), e.TargetName, "deleted", ct);
        return HasilTulis.Sukses(e.TargetCode);
    }

    public async Task<HasilTulis> RestoreTargetAsync(string code, CancellationToken ct)
    {
        var e = await _db.MejaTarget.FirstOrDefaultAsync(t => t.TargetCode == code && t.TargetSoftdeleted == 1, ct);
        if (e is null) return HasilTulis.Gagal("target tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        e.TargetSoftdeleted = 0; e.TargetUpdatedat = DateStamp.Now();
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Restore, LogService.Target(CoreTables.Target, e.TargetCode), "deleted", e.TargetName, ct);
        return HasilTulis.Sukses(e.TargetCode);
    }

    public async Task<HasilTulis> PermanentTargetAsync(string code, CancellationToken ct)
    {
        var e = await _db.MejaTarget.FirstOrDefaultAsync(t => t.TargetCode == code && t.TargetSoftdeleted == 1, ct);
        if (e is null) return HasilTulis.Gagal("target tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        _db.MejaTarget.Remove(e);
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Delete, LogService.Target(CoreTables.Target, e.TargetCode), e.TargetName, "permanent", ct);
        return HasilTulis.Sukses(code);
    }

    // ---------- PENGGUNA ----------
    public async Task<HasilTulis> CreatePenggunaAsync(string email, string password, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return HasilTulis.Gagal("email dan password wajib diisi");
        var izin = await _otoritas.BolehTulisAsync(CoreTables.Pengguna, HakAksi.Create, ct);
        if (!izin.Ok) return HasilTulis.Gagal(izin.Pesan ?? "tidak berhak", 403);
        if (await _db.MejaPengguna.AnyAsync(p => p.PenggunaEmail == email && p.PenggunaSoftdeleted == 0, ct))
            return HasilTulis.Gagal("email sudah terdaftar", 409, "KONFLIK");
        var now = DateStamp.Now();
        var e = new MejaPengguna
        {
            PenggunaCode = CodeGenerator.Next("meja_pengguna"),
            PenggunaEmail = email,
            PenggunaPassword = BCrypt.Net.BCrypt.HashPassword(password),
            PenggunaNonaktif = 0, PenggunaSoftdeleted = 0,
            PenggunaCreatedat = now, PenggunaUpdatedat = now,
        };
        _db.MejaPengguna.Add(e);
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Create, LogService.Target(CoreTables.Pengguna, e.PenggunaCode), "-", e.PenggunaEmail, ct);
        return HasilTulis.Sukses(e.PenggunaCode);
    }

    public async Task<HasilTulis> UpdatePenggunaAsync(string code, string email, int? nonaktif, CancellationToken ct)
    {
        var e = await _db.MejaPengguna.FirstOrDefaultAsync(p => p.PenggunaCode == code && p.PenggunaSoftdeleted == 0, ct);
        if (e is null) return HasilTulis.Gagal("pengguna tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        var izin = await _otoritas.BolehTulisAsync(CoreTables.Pengguna, HakAksi.Update, ct);
        if (!izin.Ok) return HasilTulis.Gagal(izin.Pesan ?? "tidak berhak", 403);
        var old = e.PenggunaEmail + "|" + e.PenggunaNonaktif;
        if (!string.IsNullOrWhiteSpace(email) && email != e.PenggunaEmail)
        {
            if (await _db.MejaPengguna.AnyAsync(p => p.PenggunaEmail == email && p.PenggunaCode != code && p.PenggunaSoftdeleted == 0, ct))
                return HasilTulis.Gagal("email sudah dipakai", 409, "KONFLIK");
            e.PenggunaEmail = email;
        }
        if (nonaktif.HasValue) e.PenggunaNonaktif = nonaktif.Value;
        e.PenggunaUpdatedat = DateStamp.Now();
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Update, LogService.Target(CoreTables.Pengguna, e.PenggunaCode), old, e.PenggunaEmail + "|" + e.PenggunaNonaktif, ct);
        return HasilTulis.Sukses(e.PenggunaCode);
    }

    public async Task<HasilTulis> SetNonaktifPenggunaAsync(string code, int nonaktif, CancellationToken ct)
    {
        var e = await _db.MejaPengguna.FirstOrDefaultAsync(p => p.PenggunaCode == code && p.PenggunaSoftdeleted == 0, ct);
        if (e is null) return HasilTulis.Gagal("pengguna tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        var izin = await _otoritas.BolehTulisAsync(CoreTables.Pengguna, HakAksi.Update, ct);
        if (!izin.Ok) return HasilTulis.Gagal(izin.Pesan ?? "tidak berhak", 403);
        var old = e.PenggunaNonaktif.ToString();
        e.PenggunaNonaktif = nonaktif; e.PenggunaUpdatedat = DateStamp.Now();
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Update, LogService.Target(CoreTables.Pengguna, e.PenggunaCode), old, e.PenggunaNonaktif.ToString(), ct);
        return HasilTulis.Sukses(e.PenggunaCode);
    }

    public async Task<HasilTulis> ResetPasswordPenggunaAsync(string code, string passwordBaru, CancellationToken ct)
    {
        var e = await _db.MejaPengguna.FirstOrDefaultAsync(p => p.PenggunaCode == code && p.PenggunaSoftdeleted == 0, ct);
        if (e is null) return HasilTulis.Gagal("pengguna tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        var izin = await _otoritas.BolehTulisAsync(CoreTables.Pengguna, HakAksi.Update, ct);
        if (!izin.Ok) return HasilTulis.Gagal(izin.Pesan ?? "tidak berhak", 403);
        e.PenggunaPassword = BCrypt.Net.BCrypt.HashPassword(passwordBaru);
        e.PenggunaUpdatedat = DateStamp.Now();
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Update, LogService.Target(CoreTables.Pengguna, e.PenggunaCode), "password", "reset", ct);
        return HasilTulis.Sukses(e.PenggunaCode);
    }

    public async Task<HasilTulis> SoftDeletePenggunaAsync(string code, CancellationToken ct)
    {
        var e = await _db.MejaPengguna.FirstOrDefaultAsync(p => p.PenggunaCode == code && p.PenggunaSoftdeleted == 0, ct);
        if (e is null) return HasilTulis.Gagal("pengguna tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        var izin = await _otoritas.BolehTulisAsync(CoreTables.Pengguna, HakAksi.Delete, ct);
        if (!izin.Ok) return HasilTulis.Gagal(izin.Pesan ?? "tidak berhak", 403);
        if (await _db.MejaHakakses.AnyAsync(h => h.PenggunaCode == code && h.HakaksesSoftdeleted == 0, ct))
            return HasilTulis.Gagal("pengguna masih punya hak akses aktif", 409, "ANTI_ORPHAN");
        e.PenggunaSoftdeleted = 1;
        e.PenggunaNonaktif = 1;
        e.PenggunaUpdatedat = DateStamp.Now();
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Softdelete, LogService.Target(CoreTables.Pengguna, e.PenggunaCode), e.PenggunaEmail, "deleted", ct);
        return HasilTulis.Sukses(e.PenggunaCode);
    }

    public async Task<HasilTulis> RestorePenggunaAsync(string code, CancellationToken ct)
    {
        var e = await _db.MejaPengguna.FirstOrDefaultAsync(p => p.PenggunaCode == code && p.PenggunaSoftdeleted == 1, ct);
        if (e is null) return HasilTulis.Gagal("pengguna tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        e.PenggunaSoftdeleted = 0;
        e.PenggunaUpdatedat = DateStamp.Now();
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Restore, LogService.Target(CoreTables.Pengguna, e.PenggunaCode), "deleted", e.PenggunaEmail, ct);
        return HasilTulis.Sukses(e.PenggunaCode);
    }

    public async Task<HasilTulis> PermanentPenggunaAsync(string code, CancellationToken ct)
    {
        var e = await _db.MejaPengguna.FirstOrDefaultAsync(p => p.PenggunaCode == code && p.PenggunaSoftdeleted == 1, ct);
        if (e is null) return HasilTulis.Gagal("pengguna tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        if (await _db.MejaHakakses.AnyAsync(h => h.PenggunaCode == code && h.HakaksesSoftdeleted == 0, ct)
            || await _db.MejaKeuangan.AnyAsync(k => k.PenggunaCode == code && k.KeuanganSoftdeleted == 0, ct)
            || await _db.MejaBiodata.AnyAsync(b => b.PenggunaCode == code && b.BiodataSoftdeleted == 0, ct))
            return HasilTulis.Gagal("pengguna masih punya data terkait", 409, "ANTI_ORPHAN");
        _db.MejaPengguna.Remove(e);
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Delete, LogService.Target(CoreTables.Pengguna, e.PenggunaCode), e.PenggunaEmail, "permanent", ct);
        return HasilTulis.Sukses(code);
    }

    // ---------- BIODATA ----------
    public async Task<HasilTulis> CreateBiodataAsync(
        string penggunaCode, string tokoCode, string jabatanCode, string fullname,
        string born, string address, string phone, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(penggunaCode) || string.IsNullOrWhiteSpace(tokoCode)
            || string.IsNullOrWhiteSpace(jabatanCode) || string.IsNullOrWhiteSpace(fullname))
            return HasilTulis.Gagal("data biodata tidak lengkap");
        var izin = await _otoritas.BolehTulisAsync(CoreTables.Biodata, HakAksi.Create, ct);
        if (!izin.Ok) return HasilTulis.Gagal(izin.Pesan ?? "tidak berhak", 403);
        if (await _db.MejaBiodata.AnyAsync(b => b.PenggunaCode == penggunaCode && b.BiodataSoftdeleted == 0, ct))
            return HasilTulis.Gagal("pengguna sudah punya biodata", 409, "KONFLIK");
        var now = DateStamp.Now();
        var e = new MejaBiodata
        { BiodataCode = CodeGenerator.Next("meja_biodata"), PenggunaCode = penggunaCode, TokoCode = tokoCode,
          JabatanCode = jabatanCode, BiodataFullname = fullname, BiodataBorn = born ?? "",
          BiodataAddress = address ?? "", BiodataPhone = phone ?? "", BiodataSoftdeleted = 0,
          BiodataCreatedat = now, BiodataUpdatedat = now };
        _db.MejaBiodata.Add(e);
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Create, LogService.Target(CoreTables.Biodata, e.BiodataCode), "-", e.BiodataFullname, ct);
        return HasilTulis.Sukses(e.BiodataCode);
    }

    public async Task<HasilTulis> UpdateBiodataAsync(
        string code, string fullname, string born, string address, string phone, CancellationToken ct)
    {
        var e = await _db.MejaBiodata.FirstOrDefaultAsync(b => b.BiodataCode == code && b.BiodataSoftdeleted == 0, ct);
        if (e is null) return HasilTulis.Gagal("biodata tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        var izin = await _otoritas.BolehTulisAsync(CoreTables.Biodata, HakAksi.Update, ct);
        if (!izin.Ok) return HasilTulis.Gagal(izin.Pesan ?? "tidak berhak", 403);
        if (!await _otoritas.PunyaAksesDataTokoAsync(e.TokoCode, ct))
            return HasilTulis.Gagal("bukan akun milik Anda", 403, "ISOLASI");
        if (string.IsNullOrWhiteSpace(fullname)) return HasilTulis.Gagal("nama biodata wajib diisi");
        var old = e.BiodataFullname;
        e.BiodataFullname = fullname; e.BiodataBorn = born ?? e.BiodataBorn;
        e.BiodataAddress = address ?? e.BiodataAddress; e.BiodataPhone = phone ?? e.BiodataPhone;
        e.BiodataUpdatedat = DateStamp.Now();
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Update, LogService.Target(CoreTables.Biodata, e.BiodataCode), old, e.BiodataFullname, ct);
        return HasilTulis.Sukses(e.BiodataCode);
    }

    public async Task<HasilTulis> SoftDeleteBiodataAsync(string code, CancellationToken ct)
    {
        var e = await _db.MejaBiodata.FirstOrDefaultAsync(b => b.BiodataCode == code && b.BiodataSoftdeleted == 0, ct);
        if (e is null) return HasilTulis.Gagal("biodata tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        var izin = await _otoritas.BolehTulisAsync(CoreTables.Biodata, HakAksi.Delete, ct);
        if (!izin.Ok) return HasilTulis.Gagal(izin.Pesan ?? "tidak berhak", 403);
        e.BiodataSoftdeleted = 1; e.BiodataUpdatedat = DateStamp.Now();
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Softdelete, LogService.Target(CoreTables.Biodata, e.BiodataCode), e.BiodataFullname, "deleted", ct);
        return HasilTulis.Sukses(e.BiodataCode);
    }

    public async Task<HasilTulis> RestoreBiodataAsync(string code, CancellationToken ct)
    {
        var e = await _db.MejaBiodata.FirstOrDefaultAsync(b => b.BiodataCode == code && b.BiodataSoftdeleted == 1, ct);
        if (e is null) return HasilTulis.Gagal("biodata tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        e.BiodataSoftdeleted = 0; e.BiodataUpdatedat = DateStamp.Now();
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Restore, LogService.Target(CoreTables.Biodata, e.BiodataCode), "deleted", e.BiodataFullname, ct);
        return HasilTulis.Sukses(e.BiodataCode);
    }

    public async Task<HasilTulis> PermanentBiodataAsync(string code, CancellationToken ct)
    {
        var e = await _db.MejaBiodata.FirstOrDefaultAsync(b => b.BiodataCode == code && b.BiodataSoftdeleted == 1, ct);
        if (e is null) return HasilTulis.Gagal("biodata tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        _db.MejaBiodata.Remove(e);
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Delete, LogService.Target(CoreTables.Biodata, e.BiodataCode), e.BiodataFullname, "permanent", ct);
        return HasilTulis.Sukses(code);
    }

    // ---------- HAK AKSES ----------
    public async Task<HasilTulis> UpsertHakaksesAsync(
        string penggunaCode, string targetCode, int read, int create, int update, int delete, int login,
        CancellationToken ct)
    {
        var izin = await _otoritas.BolehTulisAsync(CoreTables.Hakakses, HakAksi.Create, ct);
        if (!izin.Ok) return HasilTulis.Gagal(izin.Pesan ?? "tidak berhak", 403);
        if (read + create + update + delete + login < 1)
            return HasilTulis.Gagal("minimal satu hak harus aktif");
        if (!await _db.MejaPengguna.AnyAsync(p => p.PenggunaCode == penggunaCode && p.PenggunaSoftdeleted == 0, ct))
            return HasilTulis.Gagal("pengguna tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        if (!await _db.MejaTarget.AnyAsync(t => t.TargetCode == targetCode && t.TargetSoftdeleted == 0, ct))
            return HasilTulis.Gagal("target tidak ditemukan", 404, "TIDAK_DITEMUKAN");

        var e = await _db.MejaHakakses
            .FirstOrDefaultAsync(h => h.PenggunaCode == penggunaCode && h.TargetCode == targetCode && h.HakaksesSoftdeleted == 0, ct);
        var now = DateStamp.Now();
        if (e is null)
        {
            e = new MejaHakakses
            { HakaksesCode = CodeGenerator.Next("meja_hakakses"), PenggunaCode = penggunaCode,
              TargetCode = targetCode, HakaksesRead = read, HakaksesCreate = create, HakaksesUpdate = update,
              HakaksesDelete = delete, HakaksesLogin = login, HakaksesSoftdeleted = 0,
              HakaksesCreatedat = now, HakaksesUpdatedat = now };
            _db.MejaHakakses.Add(e);
        }
        else
        {
            var old = $"{e.HakaksesRead}{e.HakaksesCreate}{e.HakaksesUpdate}{e.HakaksesDelete}{e.HakaksesLogin}";
            e.HakaksesRead = read; e.HakaksesCreate = create; e.HakaksesUpdate = update;
            e.HakaksesDelete = delete; e.HakaksesLogin = login; e.HakaksesUpdatedat = now;
            _ = old;
        }
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Create, LogService.Target(CoreTables.Hakakses, e.HakaksesCode), "-", penggunaCode + "/" + targetCode, ct);
        return HasilTulis.Sukses(e.HakaksesCode);
    }

    public async Task<HasilTulis> SoftDeleteHakaksesAsync(string code, CancellationToken ct)
    {
        var e = await _db.MejaHakakses.FirstOrDefaultAsync(h => h.HakaksesCode == code && h.HakaksesSoftdeleted == 0, ct);
        if (e is null) return HasilTulis.Gagal("hak akses tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        var izin = await _otoritas.BolehTulisAsync(CoreTables.Hakakses, HakAksi.Delete, ct);
        if (!izin.Ok) return HasilTulis.Gagal(izin.Pesan ?? "tidak berhak", 403);
        e.HakaksesSoftdeleted = 1; e.HakaksesUpdatedat = DateStamp.Now();
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Softdelete, LogService.Target(CoreTables.Hakakses, e.HakaksesCode), "-", "deleted", ct);
        return HasilTulis.Sukses(e.HakaksesCode);
    }

    public async Task<HasilTulis> RestoreHakaksesAsync(string code, CancellationToken ct)
    {
        var e = await _db.MejaHakakses.FirstOrDefaultAsync(h => h.HakaksesCode == code && h.HakaksesSoftdeleted == 1, ct);
        if (e is null) return HasilTulis.Gagal("hak akses tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        e.HakaksesSoftdeleted = 0; e.HakaksesUpdatedat = DateStamp.Now();
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Restore, LogService.Target(CoreTables.Hakakses, e.HakaksesCode), "deleted", "-", ct);
        return HasilTulis.Sukses(e.HakaksesCode);
    }

    public async Task<HasilTulis> PermanentHakaksesAsync(string code, CancellationToken ct)
    {
        var e = await _db.MejaHakakses.FirstOrDefaultAsync(h => h.HakaksesCode == code && h.HakaksesSoftdeleted == 1, ct);
        if (e is null) return HasilTulis.Gagal("hak akses tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        _db.MejaHakakses.Remove(e);
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Delete, LogService.Target(CoreTables.Hakakses, e.HakaksesCode), "-", "permanent", ct);
        return HasilTulis.Sukses(code);
    }

    // ---------- KEUANGAN ----------
    public async Task<HasilTulis> CreateKeuanganAsync(
        string penggunaCode, string tokoCode, decimal nominal, string judul, string deskripsi,
        string status, string tempat, string waktucatat, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(penggunaCode) || string.IsNullOrWhiteSpace(tokoCode)
            || string.IsNullOrWhiteSpace(judul) || nominal <= 0)
            return HasilTulis.Gagal("data keuangan tidak lengkap");
        if (!KeuanganStatus.IsValid(status) || !KeuanganTempat.IsValid(tempat))
            return HasilTulis.Gagal("status atau tempat tidak valid");

        var izin = await _otoritas.BolehTulisAsync(CoreTables.Keuangan, HakAksi.Create, ct);
        if (!izin.Ok) return HasilTulis.Gagal(izin.Pesan ?? "tidak berhak", 403);
        if (!await _otoritas.PunyaAksesDataTokoAsync(tokoCode, ct))
            return HasilTulis.Gagal("bukan toko milik Anda", 403, "ISOLASI");

        var now = DateStamp.Now();
        var e = new MejaKeuangan
        { KeuanganCode = CodeGenerator.Next("meja_keuangan"), PenggunaCode = penggunaCode, TokoCode = tokoCode,
          KeuanganNominal = nominal, KeuanganJudul = judul, KeuanganDeskripsi = deskripsi ?? "",
          KeuanganStatus = status!, KeuanganTempat = tempat!, KeuanganWaktucatat = waktucatat ?? now,
          KeuanganSoftdeleted = 0, KeuanganCreatedat = now, KeuanganUpdatedat = now };
        _db.MejaKeuangan.Add(e);
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Create, LogService.Target(CoreTables.Keuangan, e.KeuanganCode), "-", $"{status} {nominal}", ct);
        return HasilTulis.Sukses(e.KeuanganCode);
    }

    public async Task<HasilTulis> UpdateKeuanganAsync(
        string code, string? judul, string? deskripsi, string? status, string? tempat, string? waktucatat, CancellationToken ct)
    {
        var e = await _db.MejaKeuangan.FirstOrDefaultAsync(k => k.KeuanganCode == code && k.KeuanganSoftdeleted == 0, ct);
        if (e is null) return HasilTulis.Gagal("keuangan tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        var izin = await _otoritas.BolehTulisAsync(CoreTables.Keuangan, HakAksi.Update, ct);
        if (!izin.Ok) return HasilTulis.Gagal(izin.Pesan ?? "tidak berhak", 403);
        if (!await _otoritas.PunyaAksesDataTokoAsync(e.TokoCode, ct))
            return HasilTulis.Gagal("bukan toko milik Anda", 403, "ISOLASI");
        if (!KeuanganStatus.IsValid(status ?? e.KeuanganStatus) || !KeuanganTempat.IsValid(tempat ?? e.KeuanganTempat))
            return HasilTulis.Gagal("status atau tempat tidak valid");

        var old = $"{e.KeuanganStatus}|{e.KeuanganNominal}";
        e.KeuanganJudul = judul ?? e.KeuanganJudul;
        e.KeuanganDeskripsi = deskripsi ?? e.KeuanganDeskripsi;
        e.KeuanganStatus = status ?? e.KeuanganStatus;
        e.KeuanganTempat = tempat ?? e.KeuanganTempat;
        e.KeuanganWaktucatat = waktucatat ?? e.KeuanganWaktucatat;
        e.KeuanganUpdatedat = DateStamp.Now();
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Update, LogService.Target(CoreTables.Keuangan, e.KeuanganCode), old, $"{e.KeuanganStatus}|{e.KeuanganNominal}", ct);
        return HasilTulis.Sukses(e.KeuanganCode);
    }

    public async Task<HasilTulis> SoftDeleteKeuanganAsync(string code, CancellationToken ct)
    {
        var e = await _db.MejaKeuangan.FirstOrDefaultAsync(k => k.KeuanganCode == code && k.KeuanganSoftdeleted == 0, ct);
        if (e is null) return HasilTulis.Gagal("keuangan tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        var izin = await _otoritas.BolehTulisAsync(CoreTables.Keuangan, HakAksi.Delete, ct);
        if (!izin.Ok) return HasilTulis.Gagal(izin.Pesan ?? "tidak berhak", 403);
        if (!await _otoritas.PunyaAksesDataTokoAsync(e.TokoCode, ct))
            return HasilTulis.Gagal("bukan toko milik Anda", 403, "ISOLASI");
        e.KeuanganSoftdeleted = 1; e.KeuanganUpdatedat = DateStamp.Now();
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Softdelete, LogService.Target(CoreTables.Keuangan, e.KeuanganCode), e.KeuanganJudul, "deleted", ct);
        return HasilTulis.Sukses(e.KeuanganCode);
    }

    public async Task<HasilTulis> RestoreKeuanganAsync(string code, CancellationToken ct)
    {
        var e = await _db.MejaKeuangan.FirstOrDefaultAsync(k => k.KeuanganCode == code && k.KeuanganSoftdeleted == 1, ct);
        if (e is null) return HasilTulis.Gagal("keuangan tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        e.KeuanganSoftdeleted = 0; e.KeuanganUpdatedat = DateStamp.Now();
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Restore, LogService.Target(CoreTables.Keuangan, e.KeuanganCode), "deleted", e.KeuanganJudul, ct);
        return HasilTulis.Sukses(e.KeuanganCode);
    }

    public async Task<HasilTulis> PermanentKeuanganAsync(string code, CancellationToken ct)
    {
        var e = await _db.MejaKeuangan.FirstOrDefaultAsync(k => k.KeuanganCode == code && k.KeuanganSoftdeleted == 1, ct);
        if (e is null) return HasilTulis.Gagal("keuangan tidak ditemukan", 404, "TIDAK_DITEMUKAN");
        _db.MejaKeuangan.Remove(e);
        await _db.SaveChangesAsync(ct);
        await _log.CatatAsync(LogAksi.Delete, LogService.Target(CoreTables.Keuangan, e.KeuanganCode), e.KeuanganJudul, "permanent", ct);
        return HasilTulis.Sukses(code);
    }
}