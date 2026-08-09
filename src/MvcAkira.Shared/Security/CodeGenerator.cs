namespace MvcAkira.Shared.Security;

public static class CodeGenerator
{
    private static readonly System.Security.Cryptography.RandomNumberGenerator Rng
        = System.Security.Cryptography.RandomNumberGenerator.Create();

    public static string Next(string tableName, DateTime? now = null)
    {
        var t = now ?? DateTime.Now;
        var random = new byte[9];
        Rng.GetBytes(random);
        var token = Convert.ToHexString(random)[..9];
        return $"{t:yyyy/MM/dd/HH:mm:ss}_{tableName}_{token}";
    }
}

public static class DateStamp
{
    public static string Now(DateTime? value = null)
        => (value ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss");
}