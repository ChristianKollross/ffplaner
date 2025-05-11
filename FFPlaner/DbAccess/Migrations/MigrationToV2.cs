using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            string sql = "";

            return sql;
        }
    }
}
