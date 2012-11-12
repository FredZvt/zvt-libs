using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zvt.Libs.Database
{
    public interface ISqlServerDatabaseUtils
    {
        List<TableColumnInfo> GetTableColumnsInfo(string tableName);
        Type GetDotNetTypeFromSqlType(SqlServerTypes sqlType);
    }
}
