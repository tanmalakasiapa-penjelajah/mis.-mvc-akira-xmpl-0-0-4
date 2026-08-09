namespace MvcAkira.Shared.Enums;

public static class KeuanganStatus
{
    public const string Masuk = "masuk";
    public const string Keluar = "keluar";
    public const string Hilang = "hilang";
    public const string Pindah = "pindah";

    public static readonly string[] All = { Masuk, Keluar, Hilang, Pindah };

    public static bool IsValid(string? value)
        => value is not null && All.Contains(value);
}

public static class KeuanganTempat
{
    public const string Tunai = "tunai";
    public const string Bank = "bank";
    public const string Ewallet = "ewallet";
    public const string Others = "others";

    public static readonly string[] All = { Tunai, Bank, Ewallet, Others };

    public static bool IsValid(string? value)
        => value is not null && All.Contains(value);
}

public static class LogAksi
{
    public const string Create = "create";
    public const string Update = "update";
    public const string Login = "login";
    public const string Logout = "logout";
    public const string Softdelete = "softdelete";
    public const string Restore = "restore";
    public const string Delete = "delete";
}

public static class HakAksi
{
    public const string Read = "read";
    public const string Create = "create";
    public const string Update = "update";
    public const string Delete = "delete";
    public const string Login = "login";
}

public static class JabatanNama
{
    public const string Developer = "developer";
    public const string Admin = "admin";
    public const string Stockkeeper = "stockkeeper";
    public const string Kitchen = "kitchen";
    public const string Kasir = "kasir";
}