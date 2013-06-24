using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zvt.Libs.Database
{
    public class SqlServerDatabaseException : Exception
    {
        public SqlServerDatabaseException(string message) : base(message) { }
        public SqlServerDatabaseException(string message, Exception innerException) : base(message, innerException) { }
    }
}
