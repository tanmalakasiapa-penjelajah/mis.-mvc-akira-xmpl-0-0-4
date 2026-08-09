using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcAkira.Shared.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MejaBiodata",
                columns: table => new
                {
                    BiodataCode = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    PenggunaCode = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TokoCode = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    JabatanCode = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    BiodataFullname = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    BiodataBorn = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    BiodataAddress = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    BiodataPhone = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    BiodataSoftdeleted = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    BiodataCreatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    BiodataUpdatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MejaBiodata", x => x.BiodataCode);
                });

            migrationBuilder.CreateTable(
                name: "MejaHakakses",
                columns: table => new
                {
                    HakaksesCode = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    PenggunaCode = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TargetCode = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    HakaksesRead = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    HakaksesCreate = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    HakaksesUpdate = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    HakaksesDelete = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    HakaksesLogin = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    HakaksesSoftdeleted = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    HakaksesCreatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    HakaksesUpdatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MejaHakakses", x => x.HakaksesCode);
                });

            migrationBuilder.CreateTable(
                name: "MejaJabatan",
                columns: table => new
                {
                    JabatanCode = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    JabatanName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    JabatanSoftdeleted = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    JabatanCreatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    JabatanUpdatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MejaJabatan", x => x.JabatanCode);
                });

            migrationBuilder.CreateTable(
                name: "MejaKeuangan",
                columns: table => new
                {
                    KeuanganCode = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    PenggunaCode = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TokoCode = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    KeuanganNominal = table.Column<decimal>(type: "NUMERIC", nullable: false),
                    KeuanganJudul = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    KeuanganDeskripsi = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    KeuanganStatus = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    KeuanganTempat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    KeuanganWaktucatat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    KeuanganSoftdeleted = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    KeuanganCreatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    KeuanganUpdatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MejaKeuangan", x => x.KeuanganCode);
                });

            migrationBuilder.CreateTable(
                name: "MejaLog",
                columns: table => new
                {
                    LogCode = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    LogPelaku = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    LogMencatat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    LogOldvalue = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    LogNewvalue = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    LogTarget = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    LogSoftdeleted = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    LogCreatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    LogUpdatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MejaLog", x => x.LogCode);
                });

            migrationBuilder.CreateTable(
                name: "MejaPengguna",
                columns: table => new
                {
                    PenggunaCode = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    PenggunaEmail = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    PenggunaPassword = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    PenggunaNonaktif = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    PenggunaSoftdeleted = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    PenggunaCreatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    PenggunaUpdatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MejaPengguna", x => x.PenggunaCode);
                });

            migrationBuilder.CreateTable(
                name: "MejaTarget",
                columns: table => new
                {
                    TargetCode = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TargetName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TargetKeterangan = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false, defaultValue: ""),
                    TargetSoftdeleted = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    TargetCreatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TargetUpdatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MejaTarget", x => x.TargetCode);
                });

            migrationBuilder.CreateTable(
                name: "MejaToko",
                columns: table => new
                {
                    TokoCode = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TokoName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TokoAddress = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TokoEmail = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TokoPhone = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TokoSoftdeleted = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    TokoCreatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TokoUpdatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MejaToko", x => x.TokoCode);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MejaBiodata_JabatanCode",
                table: "MejaBiodata",
                column: "JabatanCode");

            migrationBuilder.CreateIndex(
                name: "IX_MejaBiodata_PenggunaCode",
                table: "MejaBiodata",
                column: "PenggunaCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MejaBiodata_TokoCode",
                table: "MejaBiodata",
                column: "TokoCode");

            migrationBuilder.CreateIndex(
                name: "IX_MejaHakakses_PenggunaCode_TargetCode",
                table: "MejaHakakses",
                columns: new[] { "PenggunaCode", "TargetCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MejaHakakses_TargetCode",
                table: "MejaHakakses",
                column: "TargetCode");

            migrationBuilder.CreateIndex(
                name: "IX_MejaKeuangan_KeuanganWaktucatat",
                table: "MejaKeuangan",
                column: "KeuanganWaktucatat");

            migrationBuilder.CreateIndex(
                name: "IX_MejaKeuangan_PenggunaCode",
                table: "MejaKeuangan",
                column: "PenggunaCode");

            migrationBuilder.CreateIndex(
                name: "IX_MejaKeuangan_TokoCode",
                table: "MejaKeuangan",
                column: "TokoCode");

            migrationBuilder.CreateIndex(
                name: "IX_MejaLog_LogCreatedat",
                table: "MejaLog",
                column: "LogCreatedat");

            migrationBuilder.CreateIndex(
                name: "IX_MejaLog_LogMencatat",
                table: "MejaLog",
                column: "LogMencatat");

            migrationBuilder.CreateIndex(
                name: "IX_MejaLog_LogPelaku",
                table: "MejaLog",
                column: "LogPelaku");

            migrationBuilder.CreateIndex(
                name: "IX_MejaPengguna_PenggunaEmail",
                table: "MejaPengguna",
                column: "PenggunaEmail",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MejaBiodata");

            migrationBuilder.DropTable(
                name: "MejaHakakses");

            migrationBuilder.DropTable(
                name: "MejaJabatan");

            migrationBuilder.DropTable(
                name: "MejaKeuangan");

            migrationBuilder.DropTable(
                name: "MejaLog");

            migrationBuilder.DropTable(
                name: "MejaPengguna");

            migrationBuilder.DropTable(
                name: "MejaTarget");

            migrationBuilder.DropTable(
                name: "MejaToko");
        }
    }
}
