using System.ComponentModel.DataAnnotations;

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
        public int Nummer { get; set; }
    }
}
