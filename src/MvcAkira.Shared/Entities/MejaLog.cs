namespace MvcAkira.Shared.Entities;

public class MejaLog
{
    public string LogCode { get; set; } = default!;
    public string LogPelaku { get; set; } = default!;
    public string LogMencatat { get; set; } = default!;
    public string LogOldvalue { get; set; } = default!;
    public string LogNewvalue { get; set; } = default!;
    public string LogTarget { get; set; } = default!;
    public int LogSoftdeleted { get; set; } = 0;
    public string LogCreatedat { get; set; } = default!;
    public string LogUpdatedat { get; set; } = default!;
}