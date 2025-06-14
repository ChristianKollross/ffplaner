﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FFPlaner.Entities;

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

    public bool IsModul1 { get; set; } = false;

    public bool IsModul2 { get; set; } = false;

    public bool IsModul3 { get; set; } = false;

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
        set
        {
            IsAngemeldet = false;
            ClearModules();
        }
    }

    [NotMapped]
    public bool IsAngemeldetNull
    {
        get { return IsAngemeldet == null; }
        set
        {
            IsAngemeldet = null;
            ClearModules();
        }
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

    public List<Modul> GetModule()
    {
        return Feuerwehrdienst.GetModule(this);
    }

    public void SetExklusivesModul(int? modulNummer)
    {
        IsModul1 = modulNummer == 1;
        IsModul2 = modulNummer == 2;
        IsModul3 = modulNummer == 3;
    }

    public void SetModul(int modulNummer, bool isChosen)
    {
        switch (modulNummer)
        {
            case 1:
                IsModul1 = isChosen;
                break;
            case 2:
                IsModul2 = isChosen;
                break;
            case 3:
                IsModul3 = isChosen;
                break;
        }
    }

    public void ClearModules()
    {
        IsModul1 = false;
        IsModul2 = false;
        IsModul3 = false;
    }

    [NotMapped]
    public string ModuleFuerHistorie
    {
        get
        {
            List<Modul> module = Feuerwehrdienst.GetModule(this);

            string result = string.Empty;

            foreach (Modul modul in module)
            {
                if (result == string.Empty)
                {
                    result = modul.NummerUndBezeichnung;
                }
                else
                {
                    result += "     +     " + modul.NummerUndBezeichnung;
                }
            }

            return result;
        }
        set { }
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
    public bool IsSelectingModuleEnabled
    {
        get { return IsAnwesend == true; }
        set { }
    }

    [NotMapped]
    public bool IsSelectingModul3Enabled
    {
        get { return IsAnwesend == true && Feuerwehrdienst.Modul3 != null;  }
        set { }
    }
}
