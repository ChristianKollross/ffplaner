using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using FFPlaner.DbAccess;
using FFPlaner.Entities;

namespace FFPlaner
{
    public partial class MainWindow : Window
    {
        private const string AppVersion = "0.3.0 alpha";
        private const double HalberBildschirmAbSeitenverhaeltnis = 1.8; // Ist der Bildschirm mindestens um diesen Faktor breiter als hoch, so wird das Fenster nur etwa auf die linke Bildschrimhälfte skaliert.

        private const int wertUnbelegterModuleInTagen = 1000; // Wurde ein Modul noch nicht belegt, wird diese Anzahl an Tagen ersatzweise angenommen.
        private const int keineAutoZuweisunginnerhalbVonTagen = 400; // Wurde ein Modul innerhalb dieser Anzahl an Tagen bereits belegt, so wird es nicht automatisch zugewiesen.

        private bool AutoUpdateAnwesenheiten = true;

        private readonly DataContext db;

        private Feuerwehrdienst? CurrentFeuerwehrdienst;
        private Person? CurrentPerson;

        public MainWindow()
        {
            db = new DataContext();

            InitializeComponent();
            SetWindowSize();

            UpdateAppInfoTab();
            LoadPersonTab();
            LoadHistorieTab();
            LoadModulTab();

            List<Feuerwehrdienst> dienste = db.Feuerwehrdienste.ToList();

            if (dienste.Count > 0 && dienste.First() != null)
            {
                CurrentFeuerwehrdienst = dienste.First();
                LoadAnwesenheitTab();
            }
        }

        private void SetWindowSize()
        {
            int screenWidth = (int)SystemParameters.PrimaryScreenWidth;
            int screenHeight = (int)SystemParameters.PrimaryScreenHeight;
            double screenProportions = screenWidth / screenHeight;

            if (screenProportions >= HalberBildschirmAbSeitenverhaeltnis)
            {
                Width = screenWidth / 2 - 50;
                Height = screenHeight - 50;
                Top = 10;
                Left = 50;
            }
            else
            {
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
            }
        }

        #region Dateipfade
        private static string GetImportsBasePath()
        {
            return System.IO.Path.Join(AppDomain.CurrentDomain.BaseDirectory, "import");
        }
        #endregion

        #region Tab-Refresh-Funktionen
        private void LoadPersonTab()
        {
            var personen = db.Personen.OrderBy(p => p.Nachname).ToList();

            PersonenGrid.ItemsSource = null;
            PersonenGrid.ItemsSource = personen;
        }

        private void LoadFeuerwehrdienstTab()
        {
            var dienste = db.Feuerwehrdienste.OrderByDescending(p => p.Datum).ToList();

            UebungenGrid.ItemsSource = null;
            UebungenGrid.ItemsSource = dienste;
        }

        private void LoadAnwesenheitTab()
        {
            var anwesenheiten = db.Anwesenheiten.Where(a => a.Feuerwehrdienst.Id == CurrentFeuerwehrdienst.Id).OrderBy(a => a.Person.Nachname).ToList();
            BerechneStatistikenFuerAnwesenheiten(anwesenheiten);

            AnwesenheitGrid.ItemsSource = null;
            AnwesenheitGrid.ItemsSource = anwesenheiten;
        }

        private void LoadModulTab()
        {
            var module = db.Module.OrderBy(m => m.Nummer).ToList();

            ModulGrid.ItemsSource = null;
            ModulGrid.ItemsSource = module;
        }

        private void LoadHistorieTab()
        {
            var personen = db.Personen.ToList();

            PersonHistorieComboBox.ItemsSource = null;
            PersonHistorieComboBox.ItemsSource = personen;
        }

        private void BerechneStatistikenFuerAnwesenheiten(List<Anwesenheit> anwesenheiten)
        {
            int i = 0;
            var module = db.Module.ToList();
            TimeSpan zeitraumBeiUnbelegtenModulen = new TimeSpan(-wertUnbelegterModuleInTagen, 0, 0, 0, 0);

            Dictionary<Modul, int> moduleSeitTagenGesamt = new Dictionary<Modul, int>();
            Dictionary<int, int> modulAuswahlAnzahl = new Dictionary<int, int>();
            modulAuswahlAnzahl[1] = 0;
            modulAuswahlAnzahl[2] = 0;
            modulAuswahlAnzahl[3] = 0;

            // Über alle Mitglieder iterieren
            foreach (Anwesenheit anwesenheit in anwesenheiten)
            {
                if (anwesenheit.IsAngemeldet == true)
                {
                    modulAuswahlAnzahl[1] += anwesenheit.IsModul1 == true ? 1 : 0;
                    modulAuswahlAnzahl[2] += anwesenheit.IsModul2 == true ? 1 : 0;
                    modulAuswahlAnzahl[3] += anwesenheit.IsModul3 == true ? 1 : 0;
                }

                Dictionary<Modul, DateTime> moduleZuletztGenommen = ErmittleWennModuleZuletztGenommenWurden(anwesenheit.Person);

                // Fehlende Module mit Standard auffüllen
                foreach (Modul modul in module)
                {
                    if (!moduleZuletztGenommen.ContainsKey(modul))
                    {
                        moduleZuletztGenommen[modul] = CurrentFeuerwehrdienst.Datum.Add(zeitraumBeiUnbelegtenModulen);
                    }
                }

                // Tage pro Modul zu Gesamtsumme addieren
                if (anwesenheit.IsAngemeldet == true)
                {
                    foreach (Modul modul in module)
                    {
                        if (!moduleSeitTagenGesamt.ContainsKey(modul))
                        {
                            moduleSeitTagenGesamt[modul] = 0;
                        }

                        double tage = (CurrentFeuerwehrdienst.Datum - moduleZuletztGenommen[modul]).TotalDays;
                        moduleSeitTagenGesamt[modul] += (int)Math.Round(tage);
                    }
                }

                var sortierteZuletztGenommeneModule = moduleZuletztGenommen.ToList();
                sortierteZuletztGenommeneModule.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

                i = 1;

                foreach (var entry in sortierteZuletztGenommeneModule.ToList())
                {
                    if (i > 3)
                    {
                        break;
                    }

                    if (moduleZuletztGenommen.ContainsKey(entry.Key))
                    {
                        double tage = (CurrentFeuerwehrdienst.Datum - moduleZuletztGenommen[entry.Key]).TotalDays;
                        anwesenheit.setBedarf(i, entry.Key.NummerUndBezeichnung + " [" + Math.Round(tage) + "]");
                        i++;
                    }
                }
            }

            var sortierteTageFuerModuleGesamtsummen = moduleSeitTagenGesamt.ToList();
            sortierteTageFuerModuleGesamtsummen.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

            string empfohleneModule = "";
            i = 3;

            foreach (var entry in sortierteTageFuerModuleGesamtsummen)
            {
                if (i <= 0)
                {
                    break;
                }

                empfohleneModule += entry.Key.NummerUndBezeichnung + " [" + entry.Value + "]  \t";
                i--;
            }

            AnwesenheitListeLabel.Content = CurrentFeuerwehrdienst.Datum.ToString("dd.MM.yyyy") + " \tEmpfohlen: \t" + empfohleneModule;
            Modul1CountLabel.Content = modulAuswahlAnzahl[1];
            Modul2CountLabel.Content = modulAuswahlAnzahl[2];
            Modul3CountLabel.Content = modulAuswahlAnzahl[3];
        }

        private Dictionary<Modul, DateTime> ErmittleWennModuleZuletztGenommenWurden(Person person)
        {
            var tempAnwesenheiten = db.Anwesenheiten.Where(a => a.Person == person && a.IsAnwesend == true);
            List<Anwesenheit> personAnwesenheiten = tempAnwesenheiten.ToList();
            Dictionary<Modul, DateTime> moduleZuletztGenommen = new Dictionary<Modul, DateTime>();

            foreach (Anwesenheit personAnwesenheit in personAnwesenheiten)
            {
                if (personAnwesenheit.Feuerwehrdienst.Datum >= CurrentFeuerwehrdienst.Datum)
                {
                    continue;
                }

                List<Modul> module = personAnwesenheit.GetModule();

                foreach (Modul modul in module)
                {
                    if (!moduleZuletztGenommen.ContainsKey(modul))
                    {
                        moduleZuletztGenommen[modul] = personAnwesenheit.Feuerwehrdienst.Datum;
                    }
                    else if (moduleZuletztGenommen[modul] < personAnwesenheit.Feuerwehrdienst.Datum)
                    {
                        moduleZuletztGenommen[modul] = personAnwesenheit.Feuerwehrdienst.Datum;
                    }
                }
            }

            return moduleZuletztGenommen;
        }

        private void AutoAssignButton_Click(object sender, RoutedEventArgs e)
        {
            var anwesenheiten = db.Anwesenheiten.Where(a => a.Feuerwehrdienst.Id == CurrentFeuerwehrdienst.Id && a.IsAngemeldet == true).OrderBy(a => a.Person.Nachname).ToList();
            List<Modul> alleAngebotenenModule = CurrentFeuerwehrdienst.GetAlleAngebotenenModule();

            foreach (Anwesenheit anwesenheit in anwesenheiten)
            {
                AutoAssignModule(anwesenheit, alleAngebotenenModule);
            }

            LoadAnwesenheitTab();
        }

        private void AutoAssignModule(Anwesenheit anwesenheit, List<Modul> alleAngebotenenModule)
        {
            Dictionary<Modul, DateTime> moduleZuletztGenommen = ErmittleWennModuleZuletztGenommenWurden(anwesenheit.Person);

            Dictionary<Modul, int> tageFuerAngeboteneModule = new Dictionary<Modul, int>();

            foreach (Modul modul in alleAngebotenenModule)
            {
                if (!moduleZuletztGenommen.ContainsKey(modul))
                {
                    tageFuerAngeboteneModule[modul] = wertUnbelegterModuleInTagen;

                    continue;
                }

                double tage = (CurrentFeuerwehrdienst.Datum - moduleZuletztGenommen[modul]).TotalDays;

                if (tage >= keineAutoZuweisunginnerhalbVonTagen)
                {
                    tageFuerAngeboteneModule[modul] = (int)Math.Round(tage);
                }
            }

            if (tageFuerAngeboteneModule.Count <= 0)
            {
                return;
            }

            var sortierteTageFuerAngeboteneModule = tageFuerAngeboteneModule.ToList();
            sortierteTageFuerAngeboteneModule.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

            int? optimaleModulNummer = CurrentFeuerwehrdienst.GetModulNumber(sortierteTageFuerAngeboteneModule[0].Key);
            anwesenheit.SetExklusivesModul(optimaleModulNummer);
        }
        #endregion

        #region Personen-Ereignisse
        private void DeletePerson_Click(object sender, RoutedEventArgs e)
        {
            var rowId = PersonenGrid.SelectedIndex;

            if (rowId <= 0)
            {
                return;
            }

            Person person = (Person)PersonenGrid.Items[rowId];

            MessageBoxResult messageBoxResult = MessageBox.Show($"Soll {person.Vorname} {person.Nachname} wirklich gelöscht werden?", "Löschen bestätigen", MessageBoxButton.YesNo);

            if (messageBoxResult != MessageBoxResult.Yes)
            {
                return;
            }

            db.Remove(person);
            db.SaveChanges();

            LoadPersonTab();
        }

        private void PersonenGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit)
            {
                return;
            }

            DataGridBoundColumn? column = e.Column as DataGridBoundColumn;

            if (column == null || column.Binding == null)
            {
                return;
            }

            var rowId = PersonenGrid.SelectedIndex;
            Person person = (Person)PersonenGrid.Items[rowId];

            TextBox textBox = (TextBox)e.EditingElement;
            string newText = textBox.Text;
            string fieldName = (column.Binding as Binding).Path.Path;

            switch (fieldName)
            {
                case "Vorname":
                    person.Vorname = newText.Trim();
                    break;
                case "Nachname":
                    person.Nachname = newText.Trim();
                    break;
            }

            if (person.Id <= 0)
            {
                db.Add(person);
            }

            db.SaveChanges();
        }

        private void PersonAktivCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox)
            {
                return;
            }

            Person person = GetSelectedPerson(sender);

            CheckBox checkbox = (CheckBox)sender;
            person.IsAktiv = checkbox.IsChecked == true;

            db.SaveChanges();
            db.UpdateAnzahlAktivePersonen();
        }

        private void PersonAtemschutzCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox)
            {
                return;
            }

            Person person = GetSelectedPerson(sender);
            person.IsAtemschutz = IsCheckboxChecked(sender);

            db.SaveChanges();
        }

        private void PersonMaschinistCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox)
            {
                return;
            }

            Person person = GetSelectedPerson(sender);
            person.IsMaschinist = IsCheckboxChecked(sender);

            db.SaveChanges();
        }

        private Person GetSelectedPerson(object sender)
        {
            var rowId = PersonenGrid.SelectedIndex;
            return (Person)PersonenGrid.Items[rowId];
        }

        private bool IsCheckboxChecked(object sender)
        {
            CheckBox checkbox = (CheckBox)sender;
            return checkbox.IsChecked == true;
        }
        #endregion

        #region Übungen-Ereignisse
        private void UebungenGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var rowId = UebungenGrid.SelectedIndex;
            CurrentFeuerwehrdienst = (Feuerwehrdienst)UebungenGrid.Items[rowId];

            string labelContent = CurrentFeuerwehrdienst.Datum.ToString("dd.MM.yyyy");
            AnwesenheitListeLabel.Content = labelContent;

            CreateMissingAnwesenheiten();
            LoadAnwesenheitTab();
            UpdatUebungsModuleComboBoxes();

            AnwesenheitTab.IsEnabled = true;
            AnwesenheitTab.IsSelected = true;
        }

        private void CreateMissingAnwesenheiten()
        {
            var aktivePersonen = db.Personen.Where(p => p.IsAktiv);

            foreach (Person person in aktivePersonen)
            {
                var anzahlExistierendePassendeAnwesenheiten = db.Anwesenheiten.Where(a => a.Feuerwehrdienst.Id == CurrentFeuerwehrdienst.Id && a.Person.Id == person.Id).Count();

                if (anzahlExistierendePassendeAnwesenheiten > 0)
                {
                    continue;
                }

                Anwesenheit anwesenheit = new Anwesenheit();
                anwesenheit.Person = person;
                anwesenheit.Feuerwehrdienst = CurrentFeuerwehrdienst;

                db.Add(anwesenheit);
            }

            db.SaveChanges();
        }
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAnwesenheitTab();
        }

        private void DiensteTab_Selected(object sender, RoutedEventArgs e)
        {
            LoadFeuerwehrdienstTab();
        }
        #endregion

        #region Anwesenheiten-Ereignisse
        private void Modul1ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentFeuerwehrdienst.Modul1 = GetSelectedModul(sender);
            db.SaveChanges();
        }

        private void Modul2ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentFeuerwehrdienst.Modul2 = GetSelectedModul(sender);
            db.SaveChanges();
        }

        private void Modul3ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentFeuerwehrdienst.Modul3 = GetSelectedModul(sender);
            db.SaveChanges();
        }

        private Modul? GetSelectedModul(object sender)
        {
            ComboBox comboBox = (ComboBox)sender;

            return comboBox.SelectedItem as Modul;
        }

        private void AutoRefreshCheckbox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            AutoUpdateAnwesenheiten = checkBox.IsChecked == true;
            RefreshButton.IsEnabled = checkBox.IsChecked != true;
        }

        /*
            DataGridRow row = (DataGridRow)AnwesenheitGrid.ItemContainerGenerator.ContainerFromItem(anwesenheit);
            var cell = AnwesenheitGrid.Columns[2];
            var cp = (ContentPresenter)cell.GetCellContent(row);
            RadioButton rb = (RadioButton)cp.ContentTemplate.FindName("RadioButtonModul1", cp);
            //rb.IsEnabled = checkbox.IsChecked == true;
        */

        private void RadioButtonAngemeldetTrue_Click(object sender, RoutedEventArgs e)
        {
            Anwesenheit anwesenheit = GetSelectedAnwesenheit();
            anwesenheit.IsAngemeldet = true;
            anwesenheit.IsAnwesend = true;
            db.SaveChanges();

            if (AutoUpdateAnwesenheiten)
            {
                LoadAnwesenheitTab();
            }
        }

        private void RadioButtonAngemeldetNull_Click(object sender, RoutedEventArgs e)
        {
            GetSelectedAnwesenheit().IsAngemeldet = null;
            db.SaveChanges();
        }

        private void RadioButtonAngemeldetFalse_Click(object sender, RoutedEventArgs e)
        {
            Anwesenheit anwesenheit = GetSelectedAnwesenheit();
            anwesenheit.IsAngemeldet = false;
            anwesenheit.IsAnwesend = false;
            db.SaveChanges();

            if (AutoUpdateAnwesenheiten)
            {
                LoadAnwesenheitTab();
            }
        }

        private void RadioButtonAnwesendFalse_Click(object sender, RoutedEventArgs e)
        {
            GetSelectedAnwesenheit().IsAnwesend = false;
            db.SaveChanges();
        }

        private void RadioButtonAnwesendNull_Click(object sender, RoutedEventArgs e)
        {
            GetSelectedAnwesenheit().IsAnwesend = null;
            db.SaveChanges();
        }

        private void RadioButtonAnwesendTrue_Click(object sender, RoutedEventArgs e)
        {
            GetSelectedAnwesenheit().IsAnwesend = true;
            db.SaveChanges();
        }

        private void RadioButton_Modul1_Click(object sender, RoutedEventArgs e)
        {
            SelectAnwesenheitModulForPersonAndSave(1);
        }

        private void RadioButton_Modul2_Click(object sender, RoutedEventArgs e)
        {
            SelectAnwesenheitModulForPersonAndSave(2);
        }

        private void RadioButton_Modul3_Click(object sender, RoutedEventArgs e)
        {
            SelectAnwesenheitModulForPersonAndSave(3);
        }

        private void SelectAnwesenheitModulForPersonAndSave(int modulNummer)
        {
            var rowId = AnwesenheitGrid.SelectedIndex;
            var obj = AnwesenheitGrid.Items[rowId];

            if (obj is not Anwesenheit)
            {
                return;
            }

            Anwesenheit anwesenheit = (Anwesenheit)AnwesenheitGrid.Items[rowId];

            anwesenheit.SetExklusivesModul(modulNummer);
            db.SaveChanges();


            if (AutoUpdateAnwesenheiten)
            {
                LoadAnwesenheitTab();
            }
        }

        private void Modul3LeerenButton_Click(object sender, RoutedEventArgs e)
        {
            Modul3ComboBox.SelectedItem = null;
            CurrentFeuerwehrdienst.Modul3 = null;
            db.SaveChanges();

            if (AutoUpdateAnwesenheiten)
            {
                LoadAnwesenheitTab();
            }
        }

        private void UpdatUebungsModuleComboBoxes()
        {
            var modulSource = db.Module.OrderBy(m => m.Nummer).ToList();

            Modul1ComboBox.ItemsSource = modulSource;
            Modul2ComboBox.ItemsSource = modulSource;
            Modul3ComboBox.ItemsSource = modulSource;

            if (CurrentFeuerwehrdienst == null)
            {
                return;
            }

            Modul1ComboBox.SelectedItem = CurrentFeuerwehrdienst.Modul1;
            Modul2ComboBox.SelectedItem = CurrentFeuerwehrdienst.Modul2;
            Modul3ComboBox.SelectedItem = CurrentFeuerwehrdienst.Modul3;
        }

        private Anwesenheit GetSelectedAnwesenheit()
        {
            var rowId = AnwesenheitGrid.SelectedIndex;
            Anwesenheit anwesenheit = (Anwesenheit)AnwesenheitGrid.Items[rowId];

            return anwesenheit;
        }
        #endregion


        #region Historie-Ereignisse
        private void PersonHistorieComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Person? person = GetSelectedHistoriePerson(sender);

            CurrentPerson = person;

            var anwesenheiten = db.Anwesenheiten.Where(a => a.Person == person && a.IsAngemeldet == true).OrderByDescending(a => a.Feuerwehrdienst.Datum).ToList();

            HistorieGrid.ItemsSource = null;
            HistorieGrid.ItemsSource = anwesenheiten;
        }

        private Person? GetSelectedHistoriePerson(object sender)
        {
            ComboBox comboBox = (ComboBox)sender;

            return comboBox.SelectedItem as Person;
        }
        #endregion

        #region Modul-Ereignisse
        private void ModulGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit)
            {
                return;
            }

            string? fieldName = GetColumnName(e);

            if (fieldName == null)
            {
                return;
            }


            Modul modul = GetSelectedModul();
            string newText = GetTextBoxText(e);

            switch (fieldName)
            {
                case "Nummer":
                    modul.Nummer = newText.Trim();
                    break;
                case "Bezeichnung":
                    modul.Bezeichnung = newText.Trim();
                    break;
            }

            if (modul.Id <= 0)
            {
                db.Add(modul);
            }

            db.SaveChanges();
            UpdatUebungsModuleComboBoxes();
        }

        private string? GetColumnName(DataGridCellEditEndingEventArgs e)
        {
            DataGridBoundColumn? column = e.Column as DataGridBoundColumn;

            if (column == null || column.Binding == null)
            {
                return null;
            }

            Binding? binding = (column.Binding as Binding);

            if (binding == null)
            {
                return null;
            }

            return binding.Path.Path;
        }

        private Modul GetSelectedModul()
        {
            var rowId = ModulGrid.SelectedIndex;

            return (Modul)ModulGrid.Items[rowId];
        }

        private static string GetTextBoxText(DataGridCellEditEndingEventArgs e)
        {
            TextBox textBox = (TextBox)e.EditingElement;

            return textBox.Text;
        }
        #endregion

        #region Info-Funktionen
        private void UpdateAppInfoTab()
        {
            string infoText = $"FFPlaner v{AppVersion}";

            infoText += "\n------------------------";
            infoText += $"\nDatenbank erstellt am: \t{db.GetDbCreatedAt()}";
            infoText += $"\nDatenbank-Version: \t{DbAccess.DataContext.DbVersion}";
            infoText += $"\n\nDatenbankpfad: \t\t{db.DbPath}";
            infoText += $"\nBackup-Pfad: \t\t{DbAccess.DataContext.GetBackupsDirectoryPath()}";
            infoText += $"\nStandard-Import-Pfad: \t{GetImportsBasePath()}";
            infoText += $"\n\nWICHTIG: Die CSV-Dateien für den Import müssen UTF-8-codiert sein. In Notepad++ (kostenlos) geht dies über das Menü Codierung -> Konvertiere zu UTF-8.";
            infoText += $"\n\nDateiformat Personen: \t\tEine Person pro Zeile, inkl. Eintrittsdatum: \tMax;Mustermann;2023-12-21";
            infoText += $"\nDateiformat für Feuerwehrdienste: \tEin Datum pro Zeile: \t\t\t2025-12-31";
            infoText += $"\nDateiformat für Module: \t\tEin Modul (Nummer und Name) pro Zeile: \t42 x;Den Sinn des Lebens finden";

            AppInfos.Text = infoText;
        }
        #endregion

        #region Importfunktionen
        private void ImportPersonenMenu_Click(object sender, RoutedEventArgs e)
        {
            string? filename = AskForImportFile("Personen importieren (UTF-8-Encoding, Format: Max;Mustermann;2025-12-31)");

            if (filename == null)
            {
                return;
            }

            db.importPersonen(filename);

            LoadPersonTab();
        }


        private void ImportFeuerwehrdiensteMenu_Click(object sender, RoutedEventArgs e)
        {
            string? filename = AskForImportFile("Feuerwehrdienste importieren (UTF-8-Encoding, Format: 2025-12-13)");

            if (filename == null)
            {
                return;
            }

            db.importFeuerwehrdienste(filename);

            LoadFeuerwehrdienstTab();
        }

        private void ImportModuleMenu_Click(object sender, RoutedEventArgs e)
        {
            string? filename = AskForImportFile("Module importieren (UTF-8-Encoding, Format: M07;Modulbezeichnung)");

            if (filename == null)
            {
                return;
            }

            db.importModule(filename);

            LoadModulTab();
        }

        private string? AskForImportFile(string dialogTitle)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV-Dateien (*.csv)|*.csv|Alle Dateien (*.*)|*.*";
            openFileDialog.Title = dialogTitle;
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            openFileDialog.InitialDirectory = GetImportsBasePath();

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }

            return null;
        }
        #endregion
    }
}