using System.ComponentModel.DataAnnotations;

namespace server
{
    public class Biodata
    {
        [Key]
        [StringLength(16)]
        public string? NIK { get; set; }

        [StringLength(100)]
        public string? Nama { get; set; }

        [StringLength(50)]
        public string? TempatLahir { get; set; }

        public DateTime? TanggalLahir { get; set; }

        [StringLength(50)]
        public string? JenisKelamin { get; set; }

        [StringLength(5)]
        public string? GolonganDarah { get; set; }

        [StringLength(255)]
        public string? Alamat { get; set; }

        [StringLength(50)]
        public string? Agama { get; set; }

        [StringLength(50)]
        public string? StatusPerkawinan { get; set; }

        [StringLength(100)]
        public string? Pekerjaan { get; set; }

        [StringLength(50)]
        public string? Kewarganegaraan { get; set; }
    }
}
