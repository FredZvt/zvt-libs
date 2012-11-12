using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zvt.Libs.Database.DataLoader.Model
{
    public class TableRegisterUnit
    {
        public TableRegisterUnit()
        {
            this.ColInfo = new TableColumnInfo();
        }

        public TableRegisterUnit(string columnName, string valueAsString)
        {
            this.ColInfo = new TableColumnInfo() { Name = columnName };
            this.ValueAsString = valueAsString;
        }

        public TableRegisterUnit(
            string colName,
            SqlServerTypes colType,
            bool colIsNullable,
            string valueAsString,
            object valueAsObject,
            string ColPkName = null,
            string ColFkName = null
            )
        {
            this.ColInfo = new TableColumnInfo(colName, colType, 0, colIsNullable, ColPkName, ColFkName);
            this.ValueAsString = valueAsString;
            this.ValueAsObject = valueAsObject;
            this.ValueType = valueAsObject.GetType();
        }

        public TableColumnInfo ColInfo { get; set; }
        public string ValueAsString { get; set; }
        public Type ValueType { get; set; }
        public object ValueAsObject { get; set; }
    }
}
