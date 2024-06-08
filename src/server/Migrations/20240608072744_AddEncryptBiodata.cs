using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class AddEncryptBiodata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EncryptBiodata");

            migrationBuilder.CreateTable(
                name: "EncryptedBiodata",
                columns: table => new
                {
                    NIK = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Nama = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TempatLahir = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TanggalLahir = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    JenisKelamin = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    GolonganDarah = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Alamat = table.Column<string>(type: "character varying(25555)", maxLength: 25555, nullable: true),
                    Agama = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    StatusPerkawinan = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Pekerjaan = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Kewarganegaraan = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncryptedBiodata", x => x.NIK);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EncryptedBiodata");

            migrationBuilder.CreateTable(
                name: "EncryptBiodata",
                columns: table => new
                {
                    NIK = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Agama = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Alamat = table.Column<string>(type: "character varying(25555)", maxLength: 25555, nullable: true),
                    GolonganDarah = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    JenisKelamin = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Kewarganegaraan = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Nama = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Pekerjaan = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    StatusPerkawinan = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TanggalLahir = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TempatLahir = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncryptBiodata", x => x.NIK);
                });
        }
    }
}
