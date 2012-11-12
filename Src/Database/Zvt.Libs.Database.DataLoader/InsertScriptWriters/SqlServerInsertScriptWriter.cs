using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zvt.Libs.Database.DataLoader.Exceptions;
using Zvt.Libs.Database.DataLoader.Model;

namespace Zvt.Libs.Database.DataLoader.InsertScriptWriters
{
    public class SqlServerInsertScriptWriter : IInsertScriptWriter
    {
        public int NumberOfSelectsByInsertCommand { get; set; }

        public SqlServerInsertScriptWriter()
        {
            this.NumberOfSelectsByInsertCommand = 10;
        }

        public string[] WriteInsertScripts(Data data)
        {
            var SQLs = new List<string>();

            foreach (var tableName in data.ReferencedTables)
            {
                var tblRegs = data.DataContent[tableName];

                bool pkExists;
                var allColumns = GetAllColumnsOfTable(tblRegs, out pkExists);

                var identityInsertPrepend = string.Empty;
                var identityInsertAppend = string.Empty;
                if (pkExists)
                {
                    identityInsertPrepend = "SET IDENTITY_INSERT " + tableName + " ON" + Environment.NewLine;
                    identityInsertAppend = "SET IDENTITY_INSERT " + tableName + " OFF" + Environment.NewLine;
                }

                var sqlInsertCommand = WriteInsertCommand(tableName, tblRegs, allColumns);

                var sqlValuesClause = new StringBuilder();
                var cursor = 0;
                do
                {
                    cursor++;
                    var reg = tblRegs[cursor - 1];

                    sqlValuesClause.Append(WriteSqlValuesClause(reg, allColumns));

                    if ((cursor % this.NumberOfSelectsByInsertCommand == 0) || (cursor == tblRegs.Count))
                    {
                        sqlValuesClause.AppendLine();

                        var sqlBatch =
                            identityInsertPrepend +
                            sqlInsertCommand +
                            sqlValuesClause.ToString() +
                            identityInsertAppend;

                        SQLs.Add(sqlBatch);

                        sqlValuesClause = new StringBuilder();
                    }
                    else if (cursor < tblRegs.Count)
                    {
                        sqlValuesClause.AppendLine(" UNION ALL");
                    }
                    else
                    {
                        sqlValuesClause.AppendLine();
                    }
                }
                while (cursor < tblRegs.Count);
            }

            return SQLs.ToArray();
        }
        internal string[] GetAllColumnsOfTable(List<TableRegister> tblRegs, out bool pkExists)
        {
            pkExists = false;
            var allCols = new List<string>();
            foreach (var tblReg in tblRegs)
            {
                foreach (var unit in tblReg.RegisterData)
                {
                    if (!allCols.Contains(unit.ColInfo.Name))
                        allCols.Add(unit.ColInfo.Name);

                    if (unit.ColInfo.IsPk)
                        pkExists = true;
                }
            }
            return allCols.ToArray();
        }
        internal string WriteInsertCommand(string tableName, List<TableRegister> tblRegs, string[] allColumns)
        {
            var sqlInsertCommand = new StringBuilder();
            sqlInsertCommand.AppendFormat("INSERT INTO [{0}] (", tableName);
            for (var tblNameCounter = 0; tblNameCounter < allColumns.Length; tblNameCounter++)
            {
                sqlInsertCommand.AppendFormat("[{0}]", allColumns[tblNameCounter]);

                if (tblNameCounter < allColumns.Length - 1)
                {
                    sqlInsertCommand.Append(", ");
                }
            }
            sqlInsertCommand.AppendLine(")");
            return sqlInsertCommand.ToString();
        }
        internal string WriteSqlValuesClause(TableRegister reg, string[] allColumns)
        {
            var sqlReg = new StringBuilder();
            sqlReg.Append("SELECT ");
            for (var i = 0; i < allColumns.Length; i++)
            {
                var colName = allColumns[i];
                var unit = reg.RegisterData.Where(u => u.ColInfo.Name == colName).FirstOrDefault();

                sqlReg.Append(WriteUnitValueInSql(unit));

                if (i < allColumns.Length - 1)
                {
                    sqlReg.Append(", ");
                }
            }
            return sqlReg.ToString();
        }
        internal string WriteUnitValueInSql(TableRegisterUnit unit)
        {
            if (unit == null)
            {
                return "NULL";
            }
            else
            {
                if (unit.ValueType == typeof(string) ||
                unit.ValueType == typeof(char) ||
                unit.ValueType == typeof(Guid))
                {
                    return "'" + unit.ValueAsString + "'";
                }
                else if (unit.ValueType == typeof(byte) ||
                         unit.ValueType == typeof(Int16) ||
                         unit.ValueType == typeof(Int32) ||
                         unit.ValueType == typeof(Int64))
                {
                    return unit.ValueAsString;
                }
                else if (unit.ValueType == typeof(decimal) ||
                         unit.ValueType == typeof(float))
                {
                    var dec = (decimal)unit.ValueAsObject;
                    var ni = new NumberFormatInfo() { NumberDecimalSeparator = "." };
                    return dec.ToString(ni);
                }
                else if (unit.ValueType == typeof(bool))
                {
                    var b = (bool)unit.ValueAsObject;
                    return (b ? "1" : "0");
                }
                else if (unit.ValueType == typeof(DateTime))
                {
                    var dt = (DateTime)unit.ValueAsObject;
                    return
                        "'" +
                        dt.Year.ToString("0000") + "-" +
                        dt.Month.ToString("00") + "-" +
                        dt.Day.ToString("00") + " " +
                        dt.Hour.ToString("00") + ":" +
                        dt.Minute.ToString("00") + ":" +
                        dt.Second.ToString("00") +
                        "'";
                }
                else
                {
                    throw new InsertScriptWriterException("SqlServerInsertScriptWriter is not ready to handle values typed as '" + unit.ValueType.FullName + "', please implement it.");
                }
            }
        }
    }
}
