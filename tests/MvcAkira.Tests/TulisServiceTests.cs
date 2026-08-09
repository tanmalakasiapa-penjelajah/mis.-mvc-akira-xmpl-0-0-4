using Moq;
using Microsoft.EntityFrameworkCore;
using MvcAkira.Shared.Data;
using MvcAkira.Shared.Enums;
using MvcAkira.Shared.Security;
using MvcAkira.Shared.Services;

namespace MvcAkira.Tests;

public class TulisServiceTests
{
    private static TulisService BuatSvc(AkiraTestDb fx, CurrentUser user)
    {
        var accessor = new Mock<ICurrentUserAccessor>();
        accessor.Setup(x => x.User).Returns(user);
        var otoritas = new OtoritasService(fx.Db, accessor.Object);
        var log = new LogService(fx.Db, accessor.Object);
        return new TulisService(fx.Db, otoritas, log);
    }

    [Fact]
    public async Task UpsertHakakses_PenggunaTidakAda_Ditolak404()
    {
        using var fx = new AkiraTestDb();
        var target = fx.TambahTarget(CoreTables.Toko);
        fx.Simpan();

        var svc = BuatSvc(fx, new CurrentUser { PenggunaCode = "S001", IsSuperuser = true });
        var r = await svc.UpsertHakaksesAsync("GHOST-USER", target.TargetCode,
            read: 1, create: 0, update: 0, delete: 0, login: 0, CancellationToken.None);

        Assert.False(r.Ok);
        Assert.Equal(404, r.Status);
        Assert.Equal("TIDAK_DITEMUKAN", r.Kode);
    }

    [Fact]
    public async Task UpsertHakakses_TargetTidakAda_Ditolak404()
    {
        using var fx = new AkiraTestDb();
        fx.TambahPengguna("user@u.dev", "U001");
        fx.Simpan();

        var svc = BuatSvc(fx, new CurrentUser { PenggunaCode = "S001", IsSuperuser = true });
        var r = await svc.UpsertHakaksesAsync("U001", "GHOST-TARGET",
            read: 1, create: 0, update: 0, delete: 0, login: 0, CancellationToken.None);

        Assert.False(r.Ok);
        Assert.Equal(404, r.Status);
    }

    [Fact]
    public async Task UpsertHakakses_DataValid_Berhasil()
    {
        using var fx = new AkiraTestDb();
        fx.TambahPengguna("user@u.dev", "U001");
        var target = fx.TambahTarget(CoreTables.Toko);
        fx.Simpan();

        var svc = BuatSvc(fx, new CurrentUser { PenggunaCode = "S001", IsSuperuser = true });
        var r = await svc.UpsertHakaksesAsync("U001", target.TargetCode,
            read: 1, create: 0, update: 0, delete: 0, login: 1, CancellationToken.None);

        Assert.True(r.Ok);
        Assert.NotNull(r.Kode);
    }

    // ---------- SOFT-DELETE PENGGUNA ----------

    [Fact]
    public async Task PenggunaYangMasihPunyaHakAkses_Ditolak409()
    {
        using var fx = new AkiraTestDb();
        fx.TambahPengguna("user@u.dev", "U001");
        fx.TambahHak("U001", fx.TambahTarget(CoreTables.Toko).TargetCode, read: 1, login: 1);
        fx.Simpan();

        var svc = BuatSvc(fx, new CurrentUser { PenggunaCode = "S001", IsSuperuser = true });
        var r = await svc.SoftDeletePenggunaAsync("U001", CancellationToken.None);

        Assert.False(r.Ok);
        Assert.Equal(409, r.Status);
        Assert.Equal("ANTI_ORPHAN", r.Kode);
    }

    [Fact]
    public async Task PenggunaSoftDeleteRestorePermanent_AlurLengkap()
    {
        using var fx = new AkiraTestDb();
        fx.TambahPengguna("user@u.dev", "U001");
        fx.Simpan();
        var svc = BuatSvc(fx, new CurrentUser { PenggunaCode = "S001", IsSuperuser = true });

        var sd = await svc.SoftDeletePenggunaAsync("U001", CancellationToken.None);
        Assert.True(sd.Ok);
        var soft = await fx.Db.MejaPengguna.FindAsync("U001");
        Assert.Equal(1, soft!.PenggunaSoftdeleted);
        Assert.Equal(1, soft.PenggunaNonaktif);

        var rs = await svc.RestorePenggunaAsync("U001", CancellationToken.None);
        Assert.True(rs.Ok);

        var rm = await svc.SoftDeletePenggunaAsync("U001", CancellationToken.None);
        Assert.True(rm.Ok);
        var pr = await svc.PermanentPenggunaAsync("U001", CancellationToken.None);
        Assert.True(pr.Ok);
        Assert.Null(await fx.Db.MejaPengguna.FindAsync("U001"));
    }

    // ---------- SOFT-DELETE HAK AKSES ----------

    [Fact]
    public async Task HakaksesSoftDeleteRestorePermanent_AlurLengkap()
    {
        using var fx = new AkiraTestDb();
        fx.TambahPengguna("user@u.dev", "U001");
        var target = fx.TambahTarget(CoreTables.Toko);
        fx.TambahHak("U001", target.TargetCode, read: 1, login: 1);
        fx.Simpan();
        var svc = BuatSvc(fx, new CurrentUser { PenggunaCode = "S001", IsSuperuser = true });

        var hak = await fx.Db.MejaHakakses.FirstAsync();
        var sd = await svc.SoftDeleteHakaksesAsync(hak.HakaksesCode, CancellationToken.None);
        Assert.True(sd.Ok);

        var rs = await svc.RestoreHakaksesAsync(hak.HakaksesCode, CancellationToken.None);
        Assert.True(rs.Ok);

        await svc.SoftDeleteHakaksesAsync(hak.HakaksesCode, CancellationToken.None);
        var pr = await svc.PermanentHakaksesAsync(hak.HakaksesCode, CancellationToken.None);
        Assert.True(pr.Ok);
        Assert.Null(await fx.Db.MejaHakakses.FindAsync(hak.HakaksesCode));
    }

    // ---------- CREATE/UPDATE ----------

    [Fact]
    public async Task CreatePengguna_HashPassword_DanKodeTerisi()
    {
        using var fx = new AkiraTestDb();
        fx.Simpan();
        var svc = BuatSvc(fx, new CurrentUser { PenggunaCode = "S001", IsSuperuser = true });

        var r = await svc.CreatePenggunaAsync("baru@u.dev", "rahasia", CancellationToken.None);
        Assert.True(r.Ok);
        Assert.NotNull(r.Kode);
        var e = await fx.Db.MejaPengguna.FindAsync(r.Kode);
        Assert.NotNull(e);
        Assert.True(BCrypt.Net.BCrypt.Verify("rahasia", e!.PenggunaPassword));
    }

    [Fact]
    public async Task UpdatePengguna_UbahEmail_NilaiTerSimpan()
    {
        using var fx = new AkiraTestDb();
        fx.TambahPengguna("lama@u.dev", "U001");
        fx.Simpan();
        var svc = BuatSvc(fx, new CurrentUser { PenggunaCode = "S001", IsSuperuser = true });

        var r = await svc.UpdatePenggunaAsync("U001", "baru@u.dev", nonaktif: 1, CancellationToken.None);
        Assert.True(r.Ok);
        var e = await fx.Db.MejaPengguna.FindAsync("U001");
        Assert.Equal("baru@u.dev", e!.PenggunaEmail);
        Assert.Equal(1, e.PenggunaNonaktif);
    }

    [Fact]
    public async Task UpdateJabatan_NamaBerubah()
    {
        using var fx = new AkiraTestDb();
        var j = fx.TambahJabatan("Staff");
        fx.Simpan();
        var svc = BuatSvc(fx, new CurrentUser { PenggunaCode = "S001", IsSuperuser = true });

        var r = await svc.UpdateJabatanAsync(j.JabatanCode, "Staff Senior", CancellationToken.None);
        Assert.True(r.Ok);
        Assert.Equal("Staff Senior", (await fx.Db.MejaJabatan.FindAsync(j.JabatanCode))!.JabatanName);
    }

    [Fact]
    public async Task UpdateTarget_NamaBerubah()
    {
        using var fx = new AkiraTestDb();
        var t = fx.TambahTarget(CoreTables.Toko);
        fx.Simpan();
        var svc = BuatSvc(fx, new CurrentUser { PenggunaCode = "S001", IsSuperuser = true });

        var r = await svc.UpdateTargetAsync(t.TargetCode, "Toko Baru", "ket", CancellationToken.None);
        Assert.True(r.Ok);
        Assert.Equal("Toko Baru", (await fx.Db.MejaTarget.FindAsync(t.TargetCode))!.TargetName);
    }

    [Fact]
    public async Task UpdateBiodata_NamaBerubah()
    {
        using var fx = new AkiraTestDb();
        var p = fx.TambahPengguna("u@u.dev", "U001");
        _ = p;
        var toko = fx.TambahToko("Toko", "T001");
        var jabatan = fx.TambahJabatan("Staff");
        fx.TambahBiodata("U001", toko.TokoCode, jabatan.JabatanCode, "Nama Lama");
        fx.Simpan();
        var svc = BuatSvc(fx, new CurrentUser { PenggunaCode = "S001", IsSuperuser = true });

        var b = await fx.Db.MejaBiodata.FirstAsync();
        var r = await svc.UpdateBiodataAsync(b.BiodataCode, "Nama Baru", null, null, null, CancellationToken.None);
        Assert.True(r.Ok);
        Assert.Equal("Nama Baru", (await fx.Db.MejaBiodata.FindAsync(b.BiodataCode))!.BiodataFullname);
    }
}
