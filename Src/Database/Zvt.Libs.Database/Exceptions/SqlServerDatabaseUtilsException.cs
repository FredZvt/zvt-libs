using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zvt.Libs.Database.Exceptions
{
    public class SqlServerDatabaseUtilsException : Exception
    {
        public SqlServerDatabaseUtilsException(string message) : base(message) { }
        public SqlServerDatabaseUtilsException(string message, Exception innerException) : base(message, innerException) { }
    }
}
