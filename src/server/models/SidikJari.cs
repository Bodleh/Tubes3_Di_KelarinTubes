using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace server
{
    public class SidikJari
    {
        [JsonPropertyName("berkasCitra")]
        [Column("berkas_citra")]
        public string? BerkasCitra { get; set; }

        [JsonPropertyName("nama")]
        [StringLength(100)]
        [Column("nama")]
        public string? Nama { get; set; }
    }
}
