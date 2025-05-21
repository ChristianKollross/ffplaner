namespace FFPlaner.DbAccess.Migrations
{
    abstract class AbstractMigration
    {
        public abstract int GetOriginVersion();
        public abstract int GetTargetVersion();
        public abstract string getMigrationSQL();

        internal string generateAddColumnSql(string tableName, string columnName, string typeDescription)
        {
            return $"ALTER TABLE {tableName} ADD COLUMN \"{columnName}\" {typeDescription};";
        }
    }
}
