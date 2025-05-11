using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace FFPlaner.Entities
{
    [DebuggerDisplay("Person {Vorname} {Nachname}")]
    public class Person
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Vorname { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Nachname { get; set; } = string.Empty;

        [Required]
        public DateTime Eintrittsdatum { get; set; }

        [Required]
        public bool IsAktiv { get; set; } = true;

        public bool? IsAtemschutz { get; set; } = false;

        public bool? IsMaschinist { get; set; } = false;

        [NotMapped]
        public string? Name
        {
            get { return Nachname + ", " + Vorname; }
            set { }
        }
    }
}
