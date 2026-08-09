using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcAkira.Shared.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "meja_biodata",
                columns: table => new
                {
                    biodata_code = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    pengguna_code = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    toko_code = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    jabatan_code = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    biodata_fullname = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    biodata_born = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    biodata_address = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    biodata_phone = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    biodata_softdeleted = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    biodata_createdat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    biodata_updatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meja_biodata", x => x.biodata_code);
                });

            migrationBuilder.CreateTable(
                name: "meja_hakakses",
                columns: table => new
                {
                    hakakses_code = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    pengguna_code = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    target_code = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    hakakses_read = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    hakakses_create = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    hakakses_update = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    hakakses_delete = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    hakakses_login = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    hakakses_softdeleted = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    hakakses_createdat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    hakakses_updatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meja_hakakses", x => x.hakakses_code);
                });

            migrationBuilder.CreateTable(
                name: "meja_jabatan",
                columns: table => new
                {
                    jabatan_code = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    jabatan_name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    jabatan_softdeleted = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    jabatan_createdat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    jabatan_updatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meja_jabatan", x => x.jabatan_code);
                });

            migrationBuilder.CreateTable(
                name: "meja_keuangan",
                columns: table => new
                {
                    keuangan_code = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    pengguna_code = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    toko_code = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    keuangan_nominal = table.Column<decimal>(type: "NUMERIC", nullable: false),
                    keuangan_judul = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    keuangan_deskripsi = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    keuangan_status = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    keuangan_tempat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    keuangan_waktucatat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    keuangan_softdeleted = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    keuangan_createdat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    keuangan_updatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meja_keuangan", x => x.keuangan_code);
                });

            migrationBuilder.CreateTable(
                name: "meja_log",
                columns: table => new
                {
                    log_code = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    log_pelaku = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    log_mencatat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    log_oldvalue = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    log_newvalue = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    log_target = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    log_softdeleted = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    log_createdat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    log_updatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meja_log", x => x.log_code);
                });

            migrationBuilder.CreateTable(
                name: "meja_pengguna",
                columns: table => new
                {
                    pengguna_code = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    pengguna_email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    pengguna_password = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    pengguna_nonaktif = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    pengguna_softdeleted = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    pengguna_createdat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    pengguna_updatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meja_pengguna", x => x.pengguna_code);
                });

            migrationBuilder.CreateTable(
                name: "meja_target",
                columns: table => new
                {
                    target_code = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    target_name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    target_keterangan = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false, defaultValue: ""),
                    target_softdeleted = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    target_createdat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    target_updatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meja_target", x => x.target_code);
                });

            migrationBuilder.CreateTable(
                name: "meja_toko",
                columns: table => new
                {
                    toko_code = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    toko_name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    toko_address = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    toko_email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    toko_phone = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    toko_softdeleted = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    toko_createdat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    toko_updatedat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meja_toko", x => x.toko_code);
                });

            migrationBuilder.CreateIndex(
                name: "IX_meja_biodata_jabatan_code",
                table: "meja_biodata",
                column: "jabatan_code");

            migrationBuilder.CreateIndex(
                name: "IX_meja_biodata_pengguna_code",
                table: "meja_biodata",
                column: "pengguna_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_meja_biodata_toko_code",
                table: "meja_biodata",
                column: "toko_code");

            migrationBuilder.CreateIndex(
                name: "IX_meja_hakakses_pengguna_code_target_code",
                table: "meja_hakakses",
                columns: new[] { "pengguna_code", "target_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_meja_hakakses_target_code",
                table: "meja_hakakses",
                column: "target_code");

            migrationBuilder.CreateIndex(
                name: "IX_meja_keuangan_keuangan_waktucatat",
                table: "meja_keuangan",
                column: "keuangan_waktucatat");

            migrationBuilder.CreateIndex(
                name: "IX_meja_keuangan_pengguna_code",
                table: "meja_keuangan",
                column: "pengguna_code");

            migrationBuilder.CreateIndex(
                name: "IX_meja_keuangan_toko_code",
                table: "meja_keuangan",
                column: "toko_code");

            migrationBuilder.CreateIndex(
                name: "IX_meja_log_log_createdat",
                table: "meja_log",
                column: "log_createdat");

            migrationBuilder.CreateIndex(
                name: "IX_meja_log_log_mencatat",
                table: "meja_log",
                column: "log_mencatat");

            migrationBuilder.CreateIndex(
                name: "IX_meja_log_log_pelaku",
                table: "meja_log",
                column: "log_pelaku");

            migrationBuilder.CreateIndex(
                name: "IX_meja_pengguna_pengguna_email",
                table: "meja_pengguna",
                column: "pengguna_email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "meja_biodata");

            migrationBuilder.DropTable(
                name: "meja_hakakses");

            migrationBuilder.DropTable(
                name: "meja_jabatan");

            migrationBuilder.DropTable(
                name: "meja_keuangan");

            migrationBuilder.DropTable(
                name: "meja_log");

            migrationBuilder.DropTable(
                name: "meja_pengguna");

            migrationBuilder.DropTable(
                name: "meja_target");

            migrationBuilder.DropTable(
                name: "meja_toko");
        }
    }
}
