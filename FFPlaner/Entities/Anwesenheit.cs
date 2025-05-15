using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FFPlaner.Entities
{
    public class Anwesenheit
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int? FeuerwehrdienstId { get; set; }

        [Required]
        public Feuerwehrdienst Feuerwehrdienst { get; set; }

        [Required]
        public int? PersonId { get; set; }

        [Required]
        public Person Person { get; set; }

        public bool? IsAngemeldet { get; set; } = null;

        public bool? IsAnwesend { get; set; } = null;

        public int? ModulNummer { get; set; }

        [NotMapped]
        public Modul? Modul
        {
            get { return Feuerwehrdienst.GetModul(ModulNummer); }
            set { }
        }

        [NotMapped]
        public string? Bedarf1 { get; set; }

        [NotMapped]
        public string? Bedarf2 { get; set; }

        [NotMapped]
        public string? Bedarf3 { get; set; }

        public void setBedarf(int nummer, string text)
        {
            switch (nummer)
            {
                case 1:
                    Bedarf1 = text; break;
                case 2:
                    Bedarf2 = text; break;
                case 3:
                    Bedarf3 = text; break;
            }
        }

        [NotMapped]
        public bool IsAngemeldetTrue
        {
            get { return IsAngemeldet == true; }
            set { IsAngemeldet = true; }
        }

        [NotMapped]
        public bool IsAngemeldetFalse
        {
            get { return IsAngemeldet == false; }
            set { IsAngemeldet = false; }
        }

        [NotMapped]
        public bool IsAngemeldetNull
        {
            get { return IsAngemeldet == null; }
            set { IsAngemeldet = null; }
        }

        [NotMapped]
        public bool IsAnwesendTrue
        {
            get { return IsAnwesend == true; }
            set { IsAnwesend = true; }
        }

        [NotMapped]
        public bool IsAnwesendFalse
        {
            get { return IsAnwesend == false; }
            set { IsAnwesend = false; }
        }

        [NotMapped]
        public bool IsAnwesendNull
        {
            get { return IsAnwesend == null; }
            set { IsAnwesend = null; }
        }

        [NotMapped]
        public bool IsModul1
        {
            get { return ModulNummer == 1; }
            set { ModulNummer = 1; }
        }

        [NotMapped]
        public bool IsModul2
        {
            get { return ModulNummer == 2; }
            set { ModulNummer = 2; }
        }

        [NotMapped]
        public bool IsModul3
        {
            get { return ModulNummer == 3; }
            set { ModulNummer = 3; }
        }

        [NotMapped]
        public string ButtonGroupAngemeldet
        {
            get { return "angemeldet" + Id; }
            set { }
        }

        [NotMapped]
        public string ButtonGroupAnwesend
        {
            get { return "anwesend" + Id; }
            set { }
        }

        [NotMapped]
        public string ButtonGroupModule
        {
            get { return "modul" + Id; }
            set { }
        }
    }
}
