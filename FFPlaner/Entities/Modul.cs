using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FFPlaner.Entities
{
    public class Modul
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Bezeichnung { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Nummer { get; set; } = string.Empty;

        [NotMapped]
        public string? NummerUndBezeichnung
        {
            get { return "(" + Nummer + ") " + Bezeichnung; }
            set { }
        }
    }
}
