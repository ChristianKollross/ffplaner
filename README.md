# FFPlaner - Feuerwehr-Übungsplaner

**Windows-Desktopanwendung zur Planung von Übungsmodulen in der Freiwilligen Feuerwehr**

## Anleitung

- (Übungs-)Module können durch Doppelklick in die leere Zeile in der Module-Liste oder per CSV-Import hinzugefügt werden. (Format siehe Info-Tab.)
- Personen (Feuerwehrleute) können durch Doppelklick in die leere Zeile in der Personen-Liste oder per CSV-Import hinzugefügt werden. (Format siehe Info-Tab.)
  Personen können über das Kontext-Menü (Rechtsklick auf die Person) gelöscht werden. Dies sollte aber nicht erfolgen, wenn für die Person schon eine Anwesenheit eingetragen wurde.
  Stattdessen sollte dann der Haken "Aktiv" für die Person entfernt werden.
- Durch einen Doppelklick auf einen Feuerwehrdienst (Übung) wird die Anwesenheitsliste für diesen Dienst anhand der aktiven Personen gefüllt und angezeigt.
  Feuerwehrdienste können ausschließlich per CSV-Import angelegt werden. (Format siehe Info-Tab.)
- Für jede Übung können zwei bis drei angebotene Module gewählt werden. Die Zusagen können mit "Zugesagt" oder "Abgesagt" vermerkt werden sowie später, ob die Person auch anwesend war. 
  - Bei Zusage wird auch automatisch angenommmen, dass die Person auch anwesend sein wird. (Dies wird allerdings nicht sofort angezeigt, falls die automatische Aktualisierung deaktiviert ist!)
  - Es kann pro Person eines der drei Module gewählt werden. Es werden pro Person und über alle Zusagen summiert die Module angezeigt, die am längsten nicht mehr belegt wurden.
    In eckigen Klammern steht jeweils die Zahl der Tage hierzu. Bei noch nicht belegten Modulen werden hier 1000 Tage angenommen.
  Der "Automatisch zuweisen"-Knopf wählt für jede Person mit Zusage das dringendere Modul aus, sofern dies nicht schon innerhalb der letzten 400 Tage belegt wurde.
- Der Historie-Tab zeigt nach Auswahl einer Person alle Dienste an, für die sich die Person je angemeldet hat. Es ist jeweils das zugewiesene Übungsmodul einsehbar sowie ob die Person auch anwesend war.
- Im Info-Tab findet sich der Ablagepfad der Datenbank (SQLite), der Pfad in den bei jedem Start die Datenbank gesichert wird, und das Format für die Import-CSV-Dateien
