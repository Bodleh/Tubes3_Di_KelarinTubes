using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace server
{
    public class Biodata
    {
        [JsonPropertyName("nik")]
        [Key]
        [StringLength(16)]
        public string? NIK { get; set; }

        [JsonPropertyName("nama")]
        [StringLength(100)]
        public string? Nama { get; set; }

        [JsonPropertyName("tempatLahir")]
        [StringLength(50)]
        public string? TempatLahir { get; set; }
        
        [JsonPropertyName("tanggalLahir")]
        public DateTime? TanggalLahir { get; set; }

        [JsonPropertyName("jenisKelamin")]
        [StringLength(50)]
        public string? JenisKelamin { get; set; }

        [JsonPropertyName("golonganDarah")]
        [StringLength(5)]
        public string? GolonganDarah { get; set; }

        [JsonPropertyName("alamat")]
        [StringLength(255)]
        public string? Alamat { get; set; }

        [JsonPropertyName("agama")]
        [StringLength(50)]
        public string? Agama { get; set; }

        [JsonPropertyName("statusPerkawinan")]
        [StringLength(50)]
        public string? StatusPerkawinan { get; set; }

        [JsonPropertyName("pekerjaan")]
        [StringLength(100)]
        public string? Pekerjaan { get; set; }

        [JsonPropertyName("kewarganegaraan")]
        [StringLength(50)]
        public string? Kewarganegaraan { get; set; }
    }
}
