using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SidikJari_Biodata_nik",
                table: "SidikJari");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SidikJari",
                table: "SidikJari");

            migrationBuilder.DropIndex(
                name: "IX_SidikJari_nik",
                table: "SidikJari");

            migrationBuilder.DeleteData(
                table: "SidikJari",
                keyColumn: "Id",
                keyColumnType: "integer",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SidikJari",
                keyColumn: "Id",
                keyColumnType: "integer",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Biodata",
                keyColumn: "NIK",
                keyValue: "1234567890123456");

            migrationBuilder.DeleteData(
                table: "Biodata",
                keyColumn: "NIK",
                keyValue: "6543210987654321");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "SidikJari");

            migrationBuilder.DropColumn(
                name: "nik",
                table: "SidikJari");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "SidikJari",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "nik",
                table: "SidikJari",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SidikJari",
                table: "SidikJari",
                column: "Id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_SidikJari_Biodata_nik",
                table: "SidikJari",
                column: "nik",
                principalTable: "Biodata",
                principalColumn: "NIK");
        }
    }
}
