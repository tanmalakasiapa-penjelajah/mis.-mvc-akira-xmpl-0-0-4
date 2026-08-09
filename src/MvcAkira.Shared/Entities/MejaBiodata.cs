namespace MvcAkira.Shared.Entities;

public class MejaBiodata
{
    public string BiodataCode { get; set; } = default!;
    public string PenggunaCode { get; set; } = default!;
    public string TokoCode { get; set; } = default!;
    public string JabatanCode { get; set; } = default!;
    public string BiodataFullname { get; set; } = default!;
    public string BiodataBorn { get; set; } = default!;
    public string BiodataAddress { get; set; } = default!;
    public string BiodataPhone { get; set; } = default!;
    public int BiodataSoftdeleted { get; set; } = 0;
    public string BiodataCreatedat { get; set; } = default!;
    public string BiodataUpdatedat { get; set; } = default!;
}