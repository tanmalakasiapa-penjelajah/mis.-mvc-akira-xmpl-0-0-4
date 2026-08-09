using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace MvcAkira.Shared.Data;

/// <summary>
/// Konvensi: semua nama kolom dikonversi ke snake_case (plan-0-0-1:
/// kolom memakai <tabel>_<nama>, contoh biodata_code, toko_name).
/// Nama tabel diatur eksplisit via ToTable("meja_*").
/// </summary>
public sealed class SnakeCaseColumnNameConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entity in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var property in entity.GetDeclaredProperties())
                property.SetColumnName(ToSnakeCase(property.Name), fromDataAnnotation: false);
        }
    }

    public static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var sb = new StringBuilder(input.Length + 4);
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsUpper(c))
            {
                if (i > 0 && !char.IsUpper(input[i - 1]))
                    sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}
