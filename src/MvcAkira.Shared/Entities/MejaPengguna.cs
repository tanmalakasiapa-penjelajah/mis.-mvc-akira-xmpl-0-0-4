namespace MvcAkira.Shared.Entities;

public class MejaPengguna
{
    public string PenggunaCode { get; set; } = default!;
    public string PenggunaEmail { get; set; } = default!;
    public string PenggunaPassword { get; set; } = default!;
    public int PenggunaNonaktif { get; set; } = 0;
    public int PenggunaSoftdeleted { get; set; } = 0;
    public string PenggunaCreatedat { get; set; } = default!;
    public string PenggunaUpdatedat { get; set; } = default!;
}