namespace MvcAkira.Shared;

public static class DbPath
{
    /// <summary>Lokasi absolut database SQLite, satu file dipakai semua service.</summary>
    public static string Absolute(string? relativeOrAbsolute = null)
    {
        if (string.IsNullOrWhiteSpace(relativeOrAbsolute))
            relativeOrAbsolute = "akira-0-0-4.db";

        // Lewatkan absolute path apa adanya.
        if (Path.IsPathRooted(relativeOrAbsolute))
            return relativeOrAbsolute;

        // Cari repo root: berisi file slnx. Naik maksimal 5 level dari AppContext.BaseDirectory.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 6 && dir is not null; i++, dir = dir.Parent)
        {
            if (dir.GetFiles("*.slnx").Any() || dir.GetFiles("*.sln").Any())
            {
                var data = Path.Combine(dir.FullName, "data");
                Directory.CreateDirectory(data);
                return Path.Combine(data, relativeOrAbsolute);
            }
        }

        var fallback = Path.Combine(AppContext.BaseDirectory, "data");
        Directory.CreateDirectory(fallback);
        return Path.Combine(fallback, relativeOrAbsolute);
    }
}