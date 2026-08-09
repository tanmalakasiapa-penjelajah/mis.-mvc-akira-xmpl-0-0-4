using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MvcAkira.Shared.Data;
using MvcAkira.Shared.Entities;
using MvcAkira.Shared.Enums;
using MvcAkira.Shared.Security;

namespace MvcAkira.Tests;

/// <summary>
/// Helper untuk membuat AkiraDbContext berbasis SQLite in-memory
/// (koneksi dibuka penuh, skema dibuat via EnsureCreated).
/// </summary>
public sealed class AkiraTestDb : IDisposable
{
    public SqliteConnection Connection { get; }
    public AkiraDbContext Db { get; }

    public AkiraTestDb()
    {
        Connection = new SqliteConnection("Data Source=:memory:");
        Connection.Open();

        var options = new DbContextOptionsBuilder<AkiraDbContext>()
            .UseSqlite(Connection)
            .Options;
        Db = new AkiraDbContext(options);
        Db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Db.Dispose();
        Connection.Dispose();
    }

    // ---------- Data referensi ----------

    public MejaToko TambahToko(string name, string code)
    {
        var now = DateStamp.Now();
        var e = new MejaToko
        {
            TokoCode = code, TokoName = name, TokoAddress = "-", TokoEmail = "x@y.z",
            TokoPhone = "-", TokoSoftdeleted = 0, TokoCreatedat = now, TokoUpdatedat = now,
        };
        Db.MejaToko.Add(e);
        return e;
    }

    public MejaPengguna TambahPengguna(string email, string code)
    {
        var now = DateStamp.Now();
        var e = new MejaPengguna
        {
            PenggunaCode = code, PenggunaEmail = email,
            PenggunaPassword = BCrypt.Net.BCrypt.HashPassword("12345678"),
            PenggunaNonaktif = 0, PenggunaSoftdeleted = 0,
            PenggunaCreatedat = now, PenggunaUpdatedat = now,
        };
        Db.MejaPengguna.Add(e);
        return e;
    }

    public MejaTarget TambahTarget(string name)
    {
        var now = DateStamp.Now();
        var e = new MejaTarget
        {
            TargetCode = CodeGenerator.Next("meja_target"), TargetName = name,
            TargetKeterangan = "-", TargetSoftdeleted = 0,
            TargetCreatedat = now, TargetUpdatedat = now,
        };
        Db.MejaTarget.Add(e);
        return e;
    }

    public MejaJabatan TambahJabatan(string name)
    {
        var now = DateStamp.Now();
        var e = new MejaJabatan
        {
            JabatanCode = CodeGenerator.Next("meja_jabatan"), JabatanName = name,
            JabatanSoftdeleted = 0, JabatanCreatedat = now, JabatanUpdatedat = now,
        };
        Db.MejaJabatan.Add(e);
        return e;
    }

    public void TambahHak(string penggunaCode, string targetCode,
        int read = 0, int create = 0, int update = 0, int delete = 0, int login = 0)
    {
        var now = DateStamp.Now();
        Db.MejaHakakses.Add(new MejaHakakses
        {
            HakaksesCode = CodeGenerator.Next("meja_hakakses"),
            PenggunaCode = penggunaCode, TargetCode = targetCode,
            HakaksesRead = read, HakaksesCreate = create, HakaksesUpdate = update,
            HakaksesDelete = delete, HakaksesLogin = login, HakaksesSoftdeleted = 0,
            HakaksesCreatedat = now, HakaksesUpdatedat = now,
        });
    }

    public void TambahBiodata(string penggunaCode, string tokoCode, string jabatanCode, string nama = "Nama")
    {
        var now = DateStamp.Now();
        Db.MejaBiodata.Add(new MejaBiodata
        {
            BiodataCode = CodeGenerator.Next("meja_biodata"),
            PenggunaCode = penggunaCode, TokoCode = tokoCode, JabatanCode = jabatanCode,
            BiodataFullname = nama, BiodataBorn = "-", BiodataAddress = "-", BiodataPhone = "-",
            BiodataSoftdeleted = 0, BiodataCreatedat = now, BiodataUpdatedat = now,
        });
    }

    public void Simpan() => Db.SaveChanges();
}