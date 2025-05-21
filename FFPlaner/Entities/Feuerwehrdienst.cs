using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FFPlaner.DbAccess;

namespace FFPlaner.Entities
{
    public class Feuerwehrdienst
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public DateTime Datum { get; set; }

        public ICollection<Anwesenheit> Anwesenheiten { get; } = new List<Anwesenheit>();

        [Required]
        public bool IsAbgesagt { get; set; } = false;

        public Modul? Modul1 { get; set; }

        public Modul? Modul2 { get; set; }

        public Modul? Modul3 { get; set; }

        public int? GetModulNumber(Modul modul)
        {
            if (Modul1 != null && Modul1.Id == modul.Id)
            {
                return 1;
            }

            if (Modul2 != null && Modul2.Id == modul.Id)
            {
                return 2;
            }

            if (Modul3 != null && Modul3.Id == modul.Id)
            {
                return 3;
            }

            return null;
        }

        public Modul? GetModul(int? modulNummer)
        {
            switch (modulNummer)
            {
                case 1:
                    return Modul1;
                case 2:
                    return Modul2;
                case 3:
                    return Modul3;
            }

            return null;
        }

        public List<Modul> GetModule(Anwesenheit anwesenheit)
        {
            List<Modul> module = new List<Modul>();

            if (anwesenheit.IsModul1 && Modul1 != null)
            {
                module.Add(Modul1);
            }

            if (anwesenheit.IsModul2 && Modul2 != null)
            {
                module.Add(Modul2);
            }

            if (anwesenheit.IsModul3 && Modul3 != null)
            {
                module.Add(Modul3);
            }

            return module;
        }

        public List<Modul> GetAlleAngebotenenModule()
        {
            List<Modul> list = new List<Modul>();

            for (int i = 1; i <= 3; i++)
            {
                if (GetModul(i) != null)
                {
                    list.Add(GetModul(i));
                }
            }

            return list;
        }

        [NotMapped]
        public string? StatistikZusagen
        {
            get
            {
                int anzahlAngemeldet = Anwesenheiten.Where(a => a.IsAngemeldet == true).Count();
                double prozentAngemeldet = DataContext.AnzahlAktivePersonen == 0 ? 0 : double.Round(anzahlAngemeldet * 100 / (double)DataContext.AnzahlAktivePersonen);

                return $"{anzahlAngemeldet}/{DataContext.AnzahlAktivePersonen} ({prozentAngemeldet}%)";
            }
            set { }
        }

        [NotMapped]
        public string? StatistikAnwesenheiten
        {
            get
            {
                int anzahlAnwesend = Anwesenheiten.Where(a => a.IsAnwesend == true).Count();
                double prozentAnwesend = DataContext.AnzahlAktivePersonen == 0 ? 0 : double.Round(anzahlAnwesend * 100 / (double)DataContext.AnzahlAktivePersonen);

                return $"{anzahlAnwesend}/{DataContext.AnzahlAktivePersonen} ({prozentAnwesend}%)";
            }
            set { }
        }
    }
}