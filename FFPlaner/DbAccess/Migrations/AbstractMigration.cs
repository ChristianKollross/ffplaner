using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFPlaner.DbAccess.Migrations
{
    abstract class AbstractMigration
    {
        public abstract int GetOriginVersion();
        public abstract int GetTargetVersion();
        public abstract string getMigrationSQL();
    }
}
