namespace MvcAkira.Shared.Entities;

public class MejaHakakses
{
    public string HakaksesCode { get; set; } = default!;
    public string PenggunaCode { get; set; } = default!;
    public string TargetCode { get; set; } = default!;
    public int HakaksesRead { get; set; } = 0;
    public int HakaksesCreate { get; set; } = 0;
    public int HakaksesUpdate { get; set; } = 0;
    public int HakaksesDelete { get; set; } = 0;
    public int HakaksesLogin { get; set; } = 0;
    public int HakaksesSoftdeleted { get; set; } = 0;
    public string HakaksesCreatedat { get; set; } = default!;
    public string HakaksesUpdatedat { get; set; } = default!;
}