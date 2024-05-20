using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server
{
    public class SidikJari
    {
        [Column("berkas_citra")]
        public string? BerkasCitra { get; set; }

        [StringLength(100)]
        [Column("nama")]
        public string? Nama { get; set; }
    }
}
