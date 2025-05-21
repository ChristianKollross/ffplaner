namespace FFPlaner.DbAccess.Migrations
{
    class MigrationToV2 : AbstractMigration
    {
        public override int GetOriginVersion()
        {
            return 1;
        }

        public override int GetTargetVersion()
        {
            return 2;
        }

        public override string getMigrationSQL()
        {
            string tableName = "Anwesenheiten";
            string isModulTypeDescription = "INTEGER NOT NULL DEFAULT 0";

            string sql = generateAddColumnSql(tableName, "IsModul1", isModulTypeDescription);
            sql += generateAddColumnSql(tableName, "IsModul2", isModulTypeDescription);
            sql += generateAddColumnSql(tableName, "IsModul3", isModulTypeDescription);

            sql += GenerateMigrationSql(1);
            sql += GenerateMigrationSql(2);
            sql += GenerateMigrationSql(3);

            sql += "ALTER TABLE Anwesenheiten DROP COLUMN ModulNummer;";

            return sql;
        }

        private string GenerateMigrationSql(int modulNummer)
        {
            return $"UPDATE Anwesenheiten SET IsModul{modulNummer} = 0; UPDATE Anwesenheiten SET IsModul{modulNummer} = 1 WHERE ModulNummer == {modulNummer};";
        }
    }
}
