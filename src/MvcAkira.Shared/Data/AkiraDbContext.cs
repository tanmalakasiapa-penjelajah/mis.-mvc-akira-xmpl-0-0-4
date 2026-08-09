using Microsoft.EntityFrameworkCore;
using MvcAkira.Shared.Entities;

namespace MvcAkira.Shared.Data;

public class AkiraDbContext : DbContext
{
    public AkiraDbContext(DbContextOptions<AkiraDbContext> options) : base(options) { }

    public DbSet<MejaToko> MejaToko { get; set; } = default!;
    public DbSet<MejaPengguna> MejaPengguna { get; set; } = default!;
    public DbSet<MejaBiodata> MejaBiodata { get; set; } = default!;
    public DbSet<MejaJabatan> MejaJabatan { get; set; } = default!;
    public DbSet<MejaTarget> MejaTarget { get; set; } = default!;
    public DbSet<MejaHakakses> MejaHakakses { get; set; } = default!;
    public DbSet<MejaKeuangan> MejaKeuangan { get; set; } = default!;
    public DbSet<MejaLog> MejaLog { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MejaToko>(e =>
        {
            e.HasKey(x => x.TokoCode);
            e.Property(x => x.TokoCode).HasMaxLength(255).IsRequired();
            e.Property(x => x.TokoName).HasMaxLength(255).IsRequired();
            e.Property(x => x.TokoAddress).HasMaxLength(255).IsRequired();
            e.Property(x => x.TokoEmail).HasMaxLength(255).IsRequired();
            e.Property(x => x.TokoPhone).HasMaxLength(255).IsRequired();
            e.Property(x => x.TokoSoftdeleted).HasDefaultValue(0);
            e.Property(x => x.TokoCreatedat).HasMaxLength(255).IsRequired();
            e.Property(x => x.TokoUpdatedat).HasMaxLength(255).IsRequired();
        });

        modelBuilder.Entity<MejaPengguna>(e =>
        {
            e.HasKey(x => x.PenggunaCode);
            e.Property(x => x.PenggunaCode).HasMaxLength(255).IsRequired();
            e.Property(x => x.PenggunaEmail).HasMaxLength(255).IsRequired();
            e.Property(x => x.PenggunaPassword).HasMaxLength(255).IsRequired();
            e.Property(x => x.PenggunaNonaktif).HasDefaultValue(0);
            e.Property(x => x.PenggunaSoftdeleted).HasDefaultValue(0);
            e.Property(x => x.PenggunaCreatedat).HasMaxLength(255).IsRequired();
            e.Property(x => x.PenggunaUpdatedat).HasMaxLength(255).IsRequired();
            e.HasIndex(x => x.PenggunaEmail).IsUnique();
        });

        modelBuilder.Entity<MejaBiodata>(e =>
        {
            e.HasKey(x => x.BiodataCode);
            e.Property(x => x.BiodataCode).HasMaxLength(255).IsRequired();
            e.Property(x => x.PenggunaCode).HasMaxLength(255).IsRequired();
            e.Property(x => x.TokoCode).HasMaxLength(255).IsRequired();
            e.Property(x => x.JabatanCode).HasMaxLength(255).IsRequired();
            e.Property(x => x.BiodataFullname).HasMaxLength(255).IsRequired();
            e.Property(x => x.BiodataBorn).HasMaxLength(255).IsRequired();
            e.Property(x => x.BiodataAddress).HasMaxLength(255).IsRequired();
            e.Property(x => x.BiodataPhone).HasMaxLength(255).IsRequired();
            e.Property(x => x.BiodataSoftdeleted).HasDefaultValue(0);
            e.Property(x => x.BiodataCreatedat).HasMaxLength(255).IsRequired();
            e.Property(x => x.BiodataUpdatedat).HasMaxLength(255).IsRequired();
            e.HasIndex(x => x.PenggunaCode).IsUnique();
            e.HasIndex(x => x.TokoCode);
            e.HasIndex(x => x.JabatanCode);
        });

        modelBuilder.Entity<MejaJabatan>(e =>
        {
            e.HasKey(x => x.JabatanCode);
            e.Property(x => x.JabatanCode).HasMaxLength(255).IsRequired();
            e.Property(x => x.JabatanName).HasMaxLength(255).IsRequired();
            e.Property(x => x.JabatanSoftdeleted).HasDefaultValue(0);
            e.Property(x => x.JabatanCreatedat).HasMaxLength(255).IsRequired();
            e.Property(x => x.JabatanUpdatedat).HasMaxLength(255).IsRequired();
        });

        modelBuilder.Entity<MejaTarget>(e =>
        {
            e.HasKey(x => x.TargetCode);
            e.Property(x => x.TargetCode).HasMaxLength(255).IsRequired();
            e.Property(x => x.TargetName).HasMaxLength(255).IsRequired();
            e.Property(x => x.TargetKeterangan).HasMaxLength(255).HasDefaultValue(string.Empty);
            e.Property(x => x.TargetSoftdeleted).HasDefaultValue(0);
            e.Property(x => x.TargetCreatedat).HasMaxLength(255).IsRequired();
            e.Property(x => x.TargetUpdatedat).HasMaxLength(255).IsRequired();
        });

        modelBuilder.Entity<MejaHakakses>(e =>
        {
            e.HasKey(x => x.HakaksesCode);
            e.Property(x => x.HakaksesCode).HasMaxLength(255).IsRequired();
            e.Property(x => x.PenggunaCode).HasMaxLength(255).IsRequired();
            e.Property(x => x.TargetCode).HasMaxLength(255).IsRequired();
            e.Property(x => x.HakaksesRead).HasDefaultValue(0);
            e.Property(x => x.HakaksesCreate).HasDefaultValue(0);
            e.Property(x => x.HakaksesUpdate).HasDefaultValue(0);
            e.Property(x => x.HakaksesDelete).HasDefaultValue(0);
            e.Property(x => x.HakaksesLogin).HasDefaultValue(0);
            e.Property(x => x.HakaksesSoftdeleted).HasDefaultValue(0);
            e.Property(x => x.HakaksesCreatedat).HasMaxLength(255).IsRequired();
            e.Property(x => x.HakaksesUpdatedat).HasMaxLength(255).IsRequired();
            e.HasIndex(x => new { x.PenggunaCode, x.TargetCode }).IsUnique();
            e.HasIndex(x => x.TargetCode);
        });

        modelBuilder.Entity<MejaKeuangan>(e =>
        {
            e.HasKey(x => x.KeuanganCode);
            e.Property(x => x.KeuanganCode).HasMaxLength(255).IsRequired();
            e.Property(x => x.PenggunaCode).HasMaxLength(255).IsRequired();
            e.Property(x => x.TokoCode).HasMaxLength(255).IsRequired();
            e.Property(x => x.KeuanganNominal).HasColumnType("NUMERIC").IsRequired();
            e.Property(x => x.KeuanganJudul).HasMaxLength(255).IsRequired();
            e.Property(x => x.KeuanganDeskripsi).HasMaxLength(255).IsRequired();
            e.Property(x => x.KeuanganStatus).HasMaxLength(255).IsRequired();
            e.Property(x => x.KeuanganTempat).HasMaxLength(255).IsRequired();
            e.Property(x => x.KeuanganWaktucatat).HasMaxLength(255).IsRequired();
            e.Property(x => x.KeuanganSoftdeleted).HasDefaultValue(0);
            e.Property(x => x.KeuanganCreatedat).HasMaxLength(255).IsRequired();
            e.Property(x => x.KeuanganUpdatedat).HasMaxLength(255).IsRequired();
            e.HasIndex(x => x.PenggunaCode);
            e.HasIndex(x => x.TokoCode);
            e.HasIndex(x => x.KeuanganWaktucatat);
        });

        modelBuilder.Entity<MejaLog>(e =>
        {
            e.HasKey(x => x.LogCode);
            e.Property(x => x.LogCode).HasMaxLength(255).IsRequired();
            e.Property(x => x.LogPelaku).HasMaxLength(255).IsRequired();
            e.Property(x => x.LogMencatat).HasMaxLength(255).IsRequired();
            e.Property(x => x.LogOldvalue).HasMaxLength(255).IsRequired();
            e.Property(x => x.LogNewvalue).HasMaxLength(255).IsRequired();
            e.Property(x => x.LogTarget).HasMaxLength(255).IsRequired();
            e.Property(x => x.LogSoftdeleted).HasDefaultValue(0);
            e.Property(x => x.LogCreatedat).HasMaxLength(255).IsRequired();
            e.Property(x => x.LogUpdatedat).HasMaxLength(255).IsRequired();
            e.HasIndex(x => x.LogPelaku);
            e.HasIndex(x => x.LogCreatedat);
            e.HasIndex(x => x.LogMencatat);
        });
    }
}