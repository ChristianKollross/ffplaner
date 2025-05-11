using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using FFPlaner.DbAccess.Migrations;
using FFPlaner.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FFPlaner.DbAccess
{
    public class DataContext : DbContext
    {
        private const string DataDir = "FFPlaner";
        private const int DbVersion = 1;

        public const string DateTimeFormat = "yyyy-MM-dd hh:mm:ss";
        private const char CsvFieldSeparator = ';';

        public const string MetaField_DbVersion = "db_version";
        public const string MetaField_DbCreatedAt = "db_created_at";

        public DbSet<Metainformation> Metainformationen { get; set; }
        public DbSet<Person> Personen { get; set; }
        public DbSet<Feuerwehrdienst> Feuerwehrdienste { get; set; }
        public DbSet<Anwesenheit> Anwesenheiten { get; set; }
        public DbSet<Modul> Module { get; set; }

        public string DbPath { get; }

        public DataContext()
        {
            DbPath = GetDatabaseFilePath();

            if (!File.Exists(DbPath))
            {
                string createSql = Database.GenerateCreateScript();

                if (createSql != null)
                {
                    File.WriteAllText(GetDatabaseCreationSQLPath(), createSql);

                    Database.ExecuteSqlRaw(createSql);
                    SetMetainformation(MetaField_DbVersion, DbVersion + "");
                    SetMetainformation(MetaField_DbCreatedAt, DateTime.Now.ToString(DateTimeFormat));
                    SetMetainformation("Credits", "Datenbank erstellt durch den Feuerwehr-Übungsplaner der Freiwilligen Feuerwehr Dettelbach");

                    MessageBox.Show("Neue Datenbank erstellt", "Information");
                    return;
                }
            }

            BackupDatabase();
            RunNecessaryMigrations();
        }

        private void RunNecessaryMigrations()
        {
            int currentDbVersion = int.Parse(GetMetainformation(MetaField_DbVersion));
            List<AbstractMigration> migrations = GetMigrations();
            bool keepLookingForMigrations = true;

            while (keepLookingForMigrations)
            {
                keepLookingForMigrations = false;

                foreach (AbstractMigration migration in migrations)
                {
                    if (currentDbVersion == migration.GetOriginVersion() && migration.GetTargetVersion() <= DbVersion)
                    {
                        MessageBox.Show($"Führe Migration von DB-Version {currentDbVersion} auf Version {migration.GetTargetVersion()} aus...", "Information");

                        Database.ExecuteSqlRaw(migration.getMigrationSQL());
                        SetMetainformation(MetaField_DbVersion, migration.GetTargetVersion() + "");

                        MessageBox.Show($"Migration von DB-Version {currentDbVersion} auf Version {migration.GetTargetVersion()} ausgeführt.", "Information");

                        currentDbVersion = migration.GetTargetVersion();
                        keepLookingForMigrations = true;
                    }
                }
            }
        }

        private List<AbstractMigration> GetMigrations()
        {
            List<AbstractMigration> migrations = new List<AbstractMigration>();

            migrations.Add(new MigrationToV2());

            return migrations;
        }

        public static string GetDataDirectoryPath()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            string path = Environment.GetFolderPath(folder);

            path = Path.Join(path, DataDir);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        public static string GetBackupsDirectoryPath()
        {
            return Path.Join(GetDataDirectoryPath(), "backups");
        }

        private static string GetDatabaseFilePath()
        {
            return Path.Join(GetDataDirectoryPath(), "ffplaner.sqlite3");
        }

        public static string GetDatabaseCreationSQLPath()
        {
            return Path.Join(GetDataDirectoryPath(), $"db_creation_{DbVersion}.sql");
        }

        public static void BackupDatabase()
        {
            string databasePath = GetDatabaseFilePath();
            string backupsDirectoryPath = GetBackupsDirectoryPath();
            string backupFilePath = Path.Join(backupsDirectoryPath, $"backup_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.sqlite3");

            if (!File.Exists(databasePath))
            {
                return;
            }

            if (!Directory.Exists(backupsDirectoryPath))
            {
                Directory.CreateDirectory(backupsDirectoryPath);
            }

            File.Copy(databasePath, backupFilePath);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={DbPath};Pooling=False");

        public void SetMetainformation(string key, string value)
        {
            Metainformation? metainformation = Metainformationen.FirstOrDefault(m => m.Key == key);

            if (metainformation == null)
            {
                metainformation = new Metainformation();
                metainformation.Key = key;
                Metainformationen.Add(metainformation);
            }

            metainformation.Value = value;
            metainformation.UpdatedAt = DateTime.Now;

            SaveChanges();
        }

        public string? GetMetainformation(string key)
        {
            return Metainformationen.FirstOrDefault(m => m.Key == key)?.Value;
        }

        public string GetDbCreatedAt()
        {
            return Metainformationen.First(m => m.Key == MetaField_DbCreatedAt).Value;
        }

        public void importPersonen(string filepath)
        {
            const int BufferSize = 128;
            using (var fileStream = File.OpenRead(filepath))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                string line;

                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.Trim().Length <= 0)
                    {
                        continue;
                    }

                    string[] fields = line.Split(CsvFieldSeparator);

                    if (fields.Length != 3)
                    {
                        MessageBox.Show("Erwartet: Max;Mustermann;2023-12-31", "Falsches Datenformat");

                        return;
                    }

                    var existierendePassendePersonen = Personen.Where(p => p.Nachname == fields[1].Trim() && p.Vorname == fields[0].Trim()).Count();

                    if (existierendePassendePersonen > 0)
                    {
                        continue;
                    }

                    Person person = new Person();
                    person.Vorname = fields[0].Trim();
                    person.Nachname = fields[1].Trim();
                    person.Eintrittsdatum = DateTime.Parse(fields[2].Trim());

                    Add(person);
                }

                SaveChanges();
            }
        }

        public void importModule(string filepath)
        {
            Regex dateRegex = new Regex(@"\d{4}\-\d{2}\-\d{2}");

            const int BufferSize = 128;
            using (var fileStream = File.OpenRead(filepath))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    line = line.Trim();

                    if (line.Trim().Length <= 0)
                    {
                        continue;
                    }

                    string[] fields = line.Split(CsvFieldSeparator);

                    if (fields.Length != 2)
                    {
                        MessageBox.Show("Erwartet: 42;Sinn des Lebens finden", "Falsches Datenformat");

                        return;
                    }

                    var existierendePassendeModule = Module.Where(m => m.Nummer == int.Parse(fields[0])).Count();

                    if (existierendePassendeModule > 0)
                    {
                        continue;
                    }

                    Modul modul = new Modul { Nummer = int.Parse(fields[0]), Bezeichnung = fields[1].Trim() };

                    Add(modul);
                }

                SaveChanges();
            }
        }

        public void importFeuerwehrdienste(string filepath)
        {
            const int BufferSize = 128;
            using (var fileStream = File.OpenRead(filepath))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.Trim().Length <= 0)
                    {
                        continue;
                    }

                    if (!DateTime.TryParse(line.Trim(), out var date))
                    {
                        MessageBox.Show("Erwartet: Datumswerte im Format 2026-12-31", "Falsches Datenformat");

                        return;
                    }

                    var existierendePassendeDienste = Feuerwehrdienste.Where(d => d.Datum == date).Count();

                    if (existierendePassendeDienste > 0)
                    {
                        continue;
                    }

                    Feuerwehrdienst feuerwehrdienst = new Feuerwehrdienst() { Datum = date };

                    Add(feuerwehrdienst);
                }

                SaveChanges();
            }
        }
    }
}