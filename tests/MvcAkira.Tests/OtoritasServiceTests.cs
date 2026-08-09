using Moq;
using MvcAkira.Shared.Data;
using MvcAkira.Shared.Enums;
using MvcAkira.Shared.Security;

namespace MvcAkira.Tests;

public class OtoritasServiceTests
{
    private static (AkiraTestDb fixture, OtoritasService svc, CurrentUser user)
        Buat(string kodetoko, bool super = false, string? jabatan = null)
    {
        var fx = new AkiraTestDb();
        var user = new CurrentUser
        {
            PenggunaCode = "U001",
            Email = "user@akira.dev",
            TokoCode = kodetoko,
            TokoName = "toko",
            Jabatan = jabatan ?? "kasir",
            IsSuperuser = super,
        };
        var accessor = new Mock<ICurrentUserAccessor>();
        accessor.Setup(x => x.User).Returns(user);
        var svc = new OtoritasService(fx.Db, accessor.Object);
        return (fx, svc, user);
    }

    [Fact]
    public async Task IsSuperuserAsync_SuperuserTrue()
    {
        var (fx, svc, _) = Buat("T1", super: true);
        Assert.True(await svc.IsSuperuserAsync());
        fx.Dispose();
    }

    [Fact]
    public async Task IsSuperuserAsync_UserBiasaFalse()
    {
        var (fx, svc, _) = Buat("T1");
        Assert.False(await svc.IsSuperuserAsync());
        fx.Dispose();
    }

    [Fact]
    public async Task BolehBacaAsync_SuperuserSelaluBenar()
    {
        var (fx, svc, _) = Buat("T1", super: true);
        fx.TambahTarget(CoreTables.Toko); fx.Simpan();

        Assert.True(await svc.BolehBacaAsync(CoreTables.Toko));
        fx.Dispose();
    }

    [Fact]
    public async Task BolehBacaAsync_FlagReadBenar_ReturnBenar()
    {
        var (fx, svc, _) = Buat("T1");
        var target = fx.TambahTarget(CoreTables.Toko);
        fx.TambahPengguna("user@u.dev", "U001");
        fx.TambahHak("U001", target.TargetCode, read: 1);
        fx.Simpan();

        Assert.True(await svc.BolehBacaAsync(CoreTables.Toko));
        fx.Dispose();
    }

    [Fact]
    public async Task BolehBacaAsync_TanpaHak_ReturnSalah()
    {
        var (fx, svc, _) = Buat("T1");
        var target = fx.TambahTarget(CoreTables.Toko);
        fx.TambahPengguna("user@u.dev", "U001");
        fx.TambahHak("U001", target.TargetCode, read: 0);
        fx.Simpan();

        Assert.False(await svc.BolehBacaAsync(CoreTables.Toko));
        fx.Dispose();
    }

    [Fact]
    public async Task BolehTulis_MasterSensitif_NonSuperuserDitolak()
    {
        var (fx, svc, _) = Buat("T1");
        var target = fx.TambahTarget(CoreTables.Toko);
        fx.TambahPengguna("user@u.dev", "U001");
        fx.TambahHak("U001", target.TargetCode, create: 1);
        fx.Simpan();

        var r = await svc.BolehTulisAsync(CoreTables.Toko, HakAksi.Create);
        Assert.False(r.Ok);
        Assert.Contains("superuser", r.Pesan);
        fx.Dispose();
    }

    [Fact]
    public async Task BolehTulis_MasterSensitif_SuperuserDiizinkan()
    {
        var (fx, svc, _) = Buat("T1", super: true);
        var r = await svc.BolehTulisAsync(CoreTables.Toko, HakAksi.Create);
        Assert.True(r.Ok);
        fx.Dispose();
    }

    [Fact]
    public async Task BolehLogin_CekHakLoginFlag()
    {
        var (fx, svc, _) = Buat("T1");
        var target = fx.TambahTarget(CoreTables.Pengguna);
        fx.TambahPengguna("newbie@u.dev", "N001");
        fx.TambahHak("N001", target.TargetCode, login: 1);
        fx.Simpan();

        Assert.True(await svc.BolehLoginAsync("N001"));
        Assert.False(await svc.BolehLoginAsync("N002"));
        fx.Dispose();
    }
}