using System.ComponentModel.DataAnnotations;

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
    }
}