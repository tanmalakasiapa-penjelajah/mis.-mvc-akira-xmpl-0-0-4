namespace MvcAkira.Shared.Entities;

public class MejaTarget
{
    public string TargetCode { get; set; } = default!;
    public string TargetName { get; set; } = default!;
    public string TargetKeterangan { get; set; } = string.Empty;
    public int TargetSoftdeleted { get; set; } = 0;
    public string TargetCreatedat { get; set; } = default!;
    public string TargetUpdatedat { get; set; } = default!;
}