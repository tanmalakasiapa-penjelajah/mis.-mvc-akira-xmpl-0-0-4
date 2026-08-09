namespace MvcAkira.Shared.Entities;

public class MejaJabatan
{
    public string JabatanCode { get; set; } = default!;
    public string JabatanName { get; set; } = default!;
    public int JabatanSoftdeleted { get; set; } = 0;
    public string JabatanCreatedat { get; set; } = default!;
    public string JabatanUpdatedat { get; set; } = default!;
}