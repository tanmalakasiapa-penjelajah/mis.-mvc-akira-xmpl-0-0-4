namespace MvcAkira.Shared.Entities;

public class MejaToko
{
    public string TokoCode { get; set; } = default!;
    public string TokoName { get; set; } = default!;
    public string TokoAddress { get; set; } = default!;
    public string TokoEmail { get; set; } = default!;
    public string TokoPhone { get; set; } = default!;
    public int TokoSoftdeleted { get; set; } = 0;
    public string TokoCreatedat { get; set; } = default!;
    public string TokoUpdatedat { get; set; } = default!;
}