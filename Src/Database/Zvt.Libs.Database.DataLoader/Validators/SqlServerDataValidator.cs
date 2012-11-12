using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zvt.Libs.Database.DataLoader.Exceptions;
using Zvt.Libs.Database.DataLoader.Model;

namespace Zvt.Libs.Database.DataLoader.Validators
{
    public class SqlServerDataValidator : IDataValidator
    {
        protected ISqlServerDatabaseUtils DbUtils { get; set; }

        public SqlServerDataValidator(ISqlServerDatabaseUtils dbUtils)
        {
            if (dbUtils == null)
                throw new ArgumentNullException("dbUtils");

            this.DbUtils = dbUtils;
        }

        public void ValidateImportedData(Data importedData)
        {
            var tableNames = importedData.ReferencedTables;

            foreach (var tableName in tableNames)
            {
                var colsInfo = this.DbUtils.GetTableColumnsInfo(tableName);
                var regsToImportFromTable = importedData.DataContent[tableName];

                foreach (var reg in regsToImportFromTable)
                {
                    if (reg.RegisterData.Count > colsInfo.Count)
                    {
                        throw new DataValidatorException("There are more TableRegisterUnits in the register than available columns in table " + tableName + ".");
                    }

                    foreach (var colInfo in colsInfo)
                    {
                        var regUnitsForColumn = reg.RegisterData.Where(x => x.ColInfo.Name == colInfo.Name);

                        if (regUnitsForColumn.Count() > 1)
                        {
                            throw new DataValidatorException("The column name '" + colInfo.Name + "' is referenced more than once in a register of table '" + tableName + "'.");
                        }
                        
                        if (!regUnitsForColumn.Any() && !colInfo.IsNullable)
                        {
                            throw new DataValidatorException("The column name '" + colInfo.Name + "' is not referenced in a register of table '" + tableName + "' but this columns is not nullable.");
                        }

                        var regUnit = regUnitsForColumn.FirstOrDefault();
                        if (regUnit != null)
                        {
                            regUnit.ValueType = DbUtils.GetDotNetTypeFromSqlType(colInfo.Type);
                            try
                            {
                                if (regUnit.ValueType == typeof(bool))
                                {
                                    regUnit.ValueAsObject = regUnit.ValueAsString != "0";
                                }
                                else if (regUnit.ValueType == typeof(decimal))
                                {
                                    var ni = new NumberFormatInfo() { NumberDecimalSeparator = "." };
                                    regUnit.ValueAsObject = decimal.Parse(regUnit.ValueAsString, ni);
                                }
                                else
                                {
                                    regUnit.ValueAsObject = Convert.ChangeType(regUnit.ValueAsString, regUnit.ValueType);
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new DataValidatorException("The value '" + regUnit.ValueAsString + "' could not be converted to type '" + regUnit.ValueType.FullName + "'.", ex);
                            }

                            regUnit.ColInfo.Type = colInfo.Type;
                            regUnit.ColInfo.IsNullable = colInfo.IsNullable;
                            regUnit.ColInfo.Length = colInfo.Length;
                            regUnit.ColInfo.IsFk = colInfo.IsFk;
                            regUnit.ColInfo.FkName = colInfo.FkName;
                            regUnit.ColInfo.IsPk = colInfo.IsPk;
                            regUnit.ColInfo.PkName = colInfo.PkName;
                        }
                    }
                }
            }
        }
    }
}
