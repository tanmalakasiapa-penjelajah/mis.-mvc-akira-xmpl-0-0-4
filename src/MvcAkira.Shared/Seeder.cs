using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MvcAkira.Shared.Data;
using MvcAkira.Shared.Entities;
using MvcAkira.Shared.Enums;
using MvcAkira.Shared.Security;

namespace MvcAkira.Shared.Data;

/// <summary>
/// Seeder data tetap deterministik (plan-0-0-1 G): 3 toko, 5 jabatan,
/// 8 target, 9 akun (Kobo superuser + PLMOG/KMMOG). Idempotent.
/// </summary>
public static class Seeder
{
    public static async Task SeedAsync(AkiraDbContext db, ILogger logger)
    {
        var now = DateStamp.Now();

        // ---------- 5 jabatan ----------
        var jabatans = new[]
        {
            JabatanNama.Developer,
            JabatanNama.Admin,
            JabatanNama.Stockkeeper,
            JabatanNama.Kitchen,
            JabatanNama.Kasir,
        };
        foreach (var nama in jabatans)
        {
            if (!await db.MejaJabatan.AnyAsync(j => j.JabatanName == nama))
                db.MejaJabatan.Add(new MejaJabatan
                {
                    JabatanCode = CodeGenerator.Next("meja_jabatan"),
                    JabatanName = nama,
                    JabatanSoftdeleted = 0,
                    JabatanCreatedat = now,
                    JabatanUpdatedat = now,
                });
        }
        await db.SaveChangesAsync();

        // ---------- 3 toko ----------
        var tokoRows = new[]
        {
            ("developer", "toko pemilik (superuser)", "dev@akira.dev", "000"),
            ("pepper lunch mog", "toko PLMOG", "plmog@akira.dev", "000001"),
            ("kimukatsu mog", "toko KMMOG", "kmmog@akira.dev", "000002"),
        };
        var tokoCodes = new Dictionary<string, string>();
        foreach (var (nama, alamat, email, telp) in tokoRows)
        {
            var ex = await db.MejaToko.AsNoTracking().FirstOrDefaultAsync(t => t.TokoName == nama);
            var code = ex?.TokoCode ?? CodeGenerator.Next("meja_toko");
            if (ex is null)
                db.MejaToko.Add(new MejaToko
                {
                    TokoCode = code,
                    TokoName = nama,
                    TokoAddress = alamat,
                    TokoEmail = email,
                    TokoPhone = telp,
                    TokoSoftdeleted = 0,
                    TokoCreatedat = now,
                    TokoUpdatedat = now,
                });
            tokoCodes[nama] = code;
        }
        await db.SaveChangesAsync();
        foreach (var t in await db.MejaToko.AsNoTracking().ToListAsync())
            if (!tokoCodes.ContainsKey(t.TokoName))
                tokoCodes[t.TokoName] = t.TokoCode;

        // ---------- 8 target (nama = nama tabel) ----------
        var targets = new[] { CoreTables.Toko, CoreTables.Pengguna, CoreTables.Biodata,
            CoreTables.Jabatan, CoreTables.Target, CoreTables.Hakakses,
            CoreTables.Keuangan, CoreTables.Log };
        var targetCodes = new Dictionary<string, string>();
        foreach (var nama in targets)
        {
            var ex = await db.MejaTarget.AsNoTracking().FirstOrDefaultAsync(t => t.TargetName == nama);
            var code = ex?.TargetCode ?? CodeGenerator.Next("meja_target");
            if (ex is null)
                db.MejaTarget.Add(new MejaTarget
                {
                    TargetCode = code,
                    TargetName = nama,
                    TargetKeterangan = "target hak akses",
                    TargetSoftdeleted = 0,
                    TargetCreatedat = now,
                    TargetUpdatedat = now,
                });
            targetCodes[nama] = code;
        }
        await db.SaveChangesAsync();

        // ---------- 9 akun ----------
        var akunDef = new[]
        {
            ("kobo.kanaeru@developer.com", "kobopawanghujan", "developer", "developer", "Kobo Kanaeru"),
            ("admin.pepper.lunch.mog@gmail.com", "12345678", "admin", "pepper lunch mog", "Admin PLM"),
            ("stockkeeper.pepper.lunch.mog@gmail.com", "12345678", "stockkeeper", "pepper lunch mog", "Stockkeeper PLM"),
            ("kitchen.pepper.lunch.mog@gmail.com", "12345678", "kitchen", "pepper lunch mog", "Kitchen PLM"),
            ("kasir.pepper.lunch.mog@gmail.com", "12345678", "kasir", "pepper lunch mog", "Kasir PLM"),
            ("admin.kimukatsu.mog@gmail.com", "12345678", "admin", "kimukatsu mog", "Admin KMMOG"),
            ("stockkeeper.kimukatsu.mog@gmail.com", "12345678", "stockkeeper", "kimukatsu mog", "Stockkeeper KMMOG"),
            ("kitchen.kimukatsu.mog@gmail.com", "12345678", "kitchen", "kimukatsu mog", "Kitchen KMMOG"),
            ("kasir.kimukatsu.mog@gmail.com", "12345678", "kasir", "kimukatsu mog", "Kasir KMMOG"),
        };
        var penggunaCode = new Dictionary<string, string>();
        foreach (var (email, pass, _, _, _) in akunDef)
        {
            var ex = await db.MejaPengguna.AsNoTracking().FirstOrDefaultAsync(x => x.PenggunaEmail == email);
            if (ex is null)
            {
                var code = CodeGenerator.Next("meja_pengguna");
                db.MejaPengguna.Add(new MejaPengguna
                {
                    PenggunaCode = code,
                    PenggunaEmail = email,
                    PenggunaPassword = BCrypt.Net.BCrypt.HashPassword(pass),
                    PenggunaNonaktif = 0,
                    PenggunaSoftdeleted = 0,
                    PenggunaCreatedat = now,
                    PenggunaUpdatedat = now,
                });
                penggunaCode[email] = code;
            }
            else
            {
                penggunaCode[email] = ex.PenggunaCode;
            }
        }
        await db.SaveChangesAsync();

        // ---------- biodata 1:1 ----------
        var jabatanCodes = (await db.MejaJabatan.AsNoTracking().ToListAsync())
            .ToDictionary(j => j.JabatanName, j => j.JabatanCode);
        var semuaPengguna = await db.MejaPengguna.AsNoTracking().ToListAsync();
        foreach (var (email, nama, jabatan, tokoNama, fullname) in akunDef)
        {
            var u = semuaPengguna.First(x => x.PenggunaEmail == email);
            if (await db.MejaBiodata.AnyAsync(b => b.PenggunaCode == u.PenggunaCode)) continue;
            db.MejaBiodata.Add(new MejaBiodata
            {
                BiodataCode = CodeGenerator.Next("meja_biodata"),
                PenggunaCode = u.PenggunaCode,
                TokoCode = tokoCodes[tokoNama],
                JabatanCode = jabatanCodes[jabatan],
                BiodataFullname = fullname,
                BiodataBorn = "2000-01-01 00:00:00",
                BiodataAddress = "-",
                BiodataPhone = "-",
                BiodataSoftdeleted = 0,
                BiodataCreatedat = now,
                BiodataUpdatedat = now,
            });
        }
        await db.SaveChangesAsync();

        // ---------- hak akses ----------
        var kobo = semuaPengguna.First(x => x.PenggunaEmail == akunDef[0].Item1);
        foreach (var nama in targets)
        {
            var code = targetCodes[nama];
            if (await db.MejaHakakses.AnyAsync(h => h.PenggunaCode == kobo.PenggunaCode && h.TargetCode == code))
                continue;
            db.MejaHakakses.Add(new MejaHakakses
            {
                HakaksesCode = CodeGenerator.Next("meja_hakakses"),
                PenggunaCode = kobo.PenggunaCode,
                TargetCode = code,
                HakaksesRead = 1,
                HakaksesCreate = 1,
                HakaksesUpdate = 1,
                HakaksesDelete = 1,
                HakaksesLogin = 1,
                HakaksesSoftdeleted = 0,
                HakaksesCreatedat = now,
                HakaksesUpdatedat = now,
            });
        }
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Seeder selesai | jabatan={j} toko={t} target={tg} pengguna={p}",
            await db.MejaJabatan.CountAsync(), await db.MejaToko.CountAsync(),
            await db.MejaTarget.CountAsync(), await db.MejaPengguna.CountAsync());
    }
}