using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class InitiateDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Biodata",
                columns: table => new
                {
                    NIK = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Nama = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TempatLahir = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TanggalLahir = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JenisKelamin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    GolonganDarah = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    Alamat = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Agama = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StatusPerkawinan = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Pekerjaan = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Kewarganegaraan = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Biodata", x => x.NIK);
                });

            migrationBuilder.CreateTable(
                name: "SidikJari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    berkas_citra = table.Column<string>(type: "text", nullable: true),
                    nama = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    nik = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SidikJari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SidikJari_Biodata_nik",
                        column: x => x.nik,
                        principalTable: "Biodata",
                        principalColumn: "NIK");
                });

            migrationBuilder.InsertData(
                table: "Biodata",
                columns: new[] { "NIK", "Agama", "Alamat", "GolonganDarah", "JenisKelamin", "Kewarganegaraan", "Nama", "Pekerjaan", "StatusPerkawinan", "TanggalLahir", "TempatLahir" },
                values: new object[,]
                {
                    { "1234567890123456", "Religion X", "123 Main St", "A", "Male", "Country X", "John Doe", "Software Engineer", "Single", new DateTime(1985, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "City A" },
                    { "6543210987654321", "Religion Y", "456 Elm St", "B", "Female", "Country Y", "Jane Doe", "Data Scientist", "Married", new DateTime(1990, 2, 2, 0, 0, 0, 0, DateTimeKind.Utc), "City B" }
                });

            migrationBuilder.InsertData(
                table: "SidikJari",
                columns: new[] { "Id", "berkas_citra", "nik", "nama" },
                values: new object[,]
                {
                    { 1, "test/contoh", "1234567890123456", "John's Fingerprint" },
                    { 2, "test/contoh", "6543210987654321", "Jane's Fingerprint" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SidikJari_nik",
                table: "SidikJari",
                column: "nik");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SidikJari");

            migrationBuilder.DropTable(
                name: "Biodata");
        }
    }
}
