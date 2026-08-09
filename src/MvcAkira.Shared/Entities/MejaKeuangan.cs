namespace MvcAkira.Shared.Entities;

public class MejaKeuangan
{
    public string KeuanganCode { get; set; } = default!;
    public string PenggunaCode { get; set; } = default!;
    public string TokoCode { get; set; } = default!;
    public decimal KeuanganNominal { get; set; }
    public string KeuanganJudul { get; set; } = default!;
    public string KeuanganDeskripsi { get; set; } = default!;
    public string KeuanganStatus { get; set; } = default!;
    public string KeuanganTempat { get; set; } = default!;
    public string KeuanganWaktucatat { get; set; } = default!;
    public int KeuanganSoftdeleted { get; set; } = 0;
    public string KeuanganCreatedat { get; set; } = default!;
    public string KeuanganUpdatedat { get; set; } = default!;
}