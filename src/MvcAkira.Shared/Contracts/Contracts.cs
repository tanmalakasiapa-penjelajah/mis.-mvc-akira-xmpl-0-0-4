using MvcAkira.Shared.Entities;

namespace MvcAkira.Shared.Contracts;

public class PageResult<T>
{
    public IReadOnlyList<T> List { get; set; } = Array.Empty<T>();
    public long Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalPages { get; set; }
}

public class ListQuery
{
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 25;
    public string? Search { get; set; }
    public string? Sort { get; set; }
    public string Dir { get; set; } = "asc";

    public static readonly int[] AllowedLimits = { 5, 25, 50, 75, 100 };
}

public record AuthResult(
    bool Success,
    string? Token = null,
    string? Email = null,
    string? Nama = null,
    string? TokoName = null,
    bool IsSuperuser = false,
    string? Error = null);

public record PenggunaView(
    string PenggunaCode,
    string PenggunaEmail,
    int PenggunaNonaktif,
    string PenggunaCreatedat);

public record BiodataView(
    string BiodataCode,
    string PenggunaCode,
    string PenggunaEmail,
    string TokoCode,
    string TokoName,
    string JabatanCode,
    string JabatanName,
    string BiodataFullname,
    string BiodataBorn,
    string BiodataAddress,
    string BiodataPhone,
    string BiodataCreatedat);

public record HakaksesView(
    string HakaksesCode,
    string PenggunaCode,
    string PenggunaEmail,
    string TargetCode,
    string TargetName,
    int HakaksesRead,
    int HakaksesCreate,
    int HakaksesUpdate,
    int HakaksesDelete,
    int HakaksesLogin);

public record KeuanganView(
    string KeuanganCode,
    string PenggunaCode,
    string PenggunaEmail,
    string TokoCode,
    string TokoName,
    decimal KeuanganNominal,
    string KeuanganJudul,
    string KeuanganDeskripsi,
    string KeuanganStatus,
    string KeuanganTempat,
    string KeuanganWaktucatat,
    string KeuanganCreatedat);

public record TokoView(
    string TokoCode,
    string TokoName,
    string TokoAddress,
    string TokoEmail,
    string TokoPhone,
    string TokoCreatedat);

public record JabatanView(
    string JabatanCode,
    string JabatanName,
    string JabatanCreatedat);

public record TargetView(
    string TargetCode,
    string TargetName,
    string TargetKeterangan,
    string TargetCreatedat);

public record LogView(
    string LogCode,
    string PelakuEmail,
    string LogMencatat,
    string LogOldvalue,
    string LogNewvalue,
    string LogTarget,
    string LogCreatedat);

public record SaldoTempat(decimal Saldo, string Tempat);

public record DashboardView(
    int JumlahToko,
    int JumlahPengguna,
    int JumlahBiodata,
    int JumlahJabatan,
    int JumlahTarget,
    decimal SaldoTotal,
    IReadOnlyList<SaldoTempat> SaldoPerTempat);

public static class Mapper
{
    public static TokoView ToView(this MejaToko e)
        => new(e.TokoCode, e.TokoName, e.TokoAddress, e.TokoEmail, e.TokoPhone, e.TokoCreatedat);

    public static JabatanView ToView(this MejaJabatan e)
        => new(e.JabatanCode, e.JabatanName, e.JabatanCreatedat);

    public static TargetView ToView(this MejaTarget e)
        => new(e.TargetCode, e.TargetName, e.TargetKeterangan, e.TargetCreatedat);

    public static PenggunaView ToView(this MejaPengguna e)
        => new(e.PenggunaCode, e.PenggunaEmail, e.PenggunaNonaktif, e.PenggunaCreatedat);
}