using Moq;
using MvcAkira.Shared.Contracts;
using MvcAkira.Shared.Data;
using MvcAkira.Shared.Entities;
using MvcAkira.Shared.Enums;
using MvcAkira.Shared.Security;
using MvcAkira.Shared.Services;

namespace MvcAkira.Tests;

public class BacaServiceTests
{
    private static BacaService BuatSvc(AkiraTestDb fx, CurrentUser user)
    {
        var accessor = new Mock<ICurrentUserAccessor>();
        accessor.Setup(x => x.User).Returns(user);
        var otoritas = new OtoritasService(fx.Db, accessor.Object);
        return new BacaService(fx.Db, otoritas);
    }

    [Fact]
    public async Task ListToko_Pagination_Tepat()
    {
        using var fx = new AkiraTestDb();
        for (var i = 1; i <= 7; i++)
            fx.TambahToko($"Toko-{i}", $"T{i}");

        fx.Simpan();

        var svc = BuatSvc(fx, new CurrentUser { PenggunaCode = "U001", IsSuperuser = true });
        var page = await svc.ListToko(new ListQuery { Page = 1, Limit = 25 }, CancellationToken.None);

        Assert.Equal(7, page.Total);
        Assert.Equal(7, page.List.Count);
        Assert.Equal(1, page.TotalPages);

        var page2 = await svc.ListToko(new ListQuery { Page = 1, Limit = 5 }, CancellationToken.None);
        Assert.Equal(5, page2.List.Count);
        Assert.Equal(2, page2.TotalPages);
    }

    [Fact]
    public async Task ListToko_LimitTidakDiizinkan_MelemparkanApiException()
    {
        using var fx = new AkiraTestDb();
        var err = await Assert.ThrowsAsync<ApiException>(async () =>
            await BuatSvc(fx, new CurrentUser { IsSuperuser = true })
                .ListToko(new ListQuery { Page = 1, Limit = 12 }, CancellationToken.None));
        Assert.Equal(400, err.StatusCode);
    }

    [Fact]
    public async Task ListKeuangan_IsolasiPerToko_UserBiasaHanyaTokonya()
    {
        using var fx = new AkiraTestDb();
        fx.TambahToko("Toko A", "TA");
        fx.TambahToko("Toko B", "TB");
        var j = fx.TambahJabatan("kasir");
        fx.TambahPengguna("kasir@tokoa.dev", "K001");
        fx.TambahPengguna("user@tokob.dev", "U001");
        fx.TambahBiodata("K001", "TA", j.JabatanCode, "Kasir A");
        fx.TambahBiodata("U001", "TB", j.JabatanCode, "User B");

        var now = DateStamp.Now();
        fx.Db.MejaKeuangan.Add(new MejaKeuangan
        {
            KeuanganCode = CodeGenerator.Next("meja_keuangan"),
            PenggunaCode = "U001", TokoCode = "TB",
            KeuanganNominal = 5000, KeuanganJudul = "uang toko lain",
            KeuanganDeskripsi = "-", KeuanganStatus = KeuanganStatus.Masuk,
            KeuanganTempat = KeuanganTempat.Tunai, KeuanganWaktucatat = now,
            KeuanganSoftdeleted = 0, KeuanganCreatedat = now, KeuanganUpdatedat = now,
        });
        fx.Simpan();

        // user biasa -> UserTokoCode = TA (dari biodatanya), jadi data toko B tak tampil
        var svc = BuatSvc(fx, new CurrentUser { PenggunaCode = "K001" });
        var page = await svc.ListKeuangan(new ListQuery { Page = 1, Limit = 25 }, CancellationToken.None);

        Assert.Equal(0, page.Total);
        Assert.Empty(page.List);
    }

    [Fact]
    public async Task ListKeuangan_Superuser_MelihatSemuaToko()
    {
        using var fx = new AkiraTestDb();
        fx.TambahToko("Toko B", "TB");
        var j = fx.TambahJabatan("kasir");
        fx.TambahPengguna("user@tokob.dev", "U001");
        fx.TambahBiodata("U001", "TB", j.JabatanCode, "User B");

        var now = DateStamp.Now();
        fx.Db.MejaKeuangan.Add(new MejaKeuangan
        {
            KeuanganCode = CodeGenerator.Next("meja_keuangan"),
            PenggunaCode = "U001", TokoCode = "TB",
            KeuanganNominal = 5000, KeuanganJudul = "uang",
            KeuanganDeskripsi = "-", KeuanganStatus = KeuanganStatus.Masuk,
            KeuanganTempat = KeuanganTempat.Tunai, KeuanganWaktucatat = now,
            KeuanganSoftdeleted = 0, KeuanganCreatedat = now, KeuanganUpdatedat = now,
        });
        fx.Simpan();

        var svc = BuatSvc(fx, new CurrentUser { PenggunaCode = "S001", IsSuperuser = true });
        var page = await svc.ListKeuangan(new ListQuery { Page = 1, Limit = 25 }, CancellationToken.None);

        Assert.Equal(1, page.Total);
    }

    [Fact]
    public async Task Dashboard_Superuser_SaldoTotalBenar()
    {
        using var fx = new AkiraTestDb();
        fx.TambahToko("Toko A", "TA");
        var j = fx.TambahJabatan("kasir");
        fx.TambahPengguna("u@t.dev", "U001");
        fx.TambahBiodata("U001", "TA", j.JabatanCode);

        var now = DateStamp.Now();
        fx.Db.MejaKeuangan.Add(new MejaKeuangan
        {
            KeuanganCode = CodeGenerator.Next("meja_keuangan"), PenggunaCode = "U001", TokoCode = "TA",
            KeuanganNominal = 100000, KeuanganJudul = "masuk", KeuanganDeskripsi = "-",
            KeuanganStatus = KeuanganStatus.Masuk, KeuanganTempat = KeuanganTempat.Tunai,
            KeuanganWaktucatat = now, KeuanganSoftdeleted = 0, KeuanganCreatedat = now, KeuanganUpdatedat = now,
        });
        fx.Db.MejaKeuangan.Add(new MejaKeuangan
        {
            KeuanganCode = CodeGenerator.Next("meja_keuangan"), PenggunaCode = "U001", TokoCode = "TA",
            KeuanganNominal = 25000, KeuanganJudul = "belanja", KeuanganDeskripsi = "-",
            KeuanganStatus = KeuanganStatus.Keluar, KeuanganTempat = KeuanganTempat.Bank,
            KeuanganWaktucatat = now, KeuanganSoftdeleted = 0, KeuanganCreatedat = now, KeuanganUpdatedat = now,
        });
        fx.Simpan();

        var svc = BuatSvc(fx, new CurrentUser { PenggunaCode = "S001", IsSuperuser = true });
        var d = await svc.DashboardAsync(CancellationToken.None);

        Assert.Equal(1, d.JumlahToko);
        Assert.Equal(75000m, d.SaldoTotal);
    }
}