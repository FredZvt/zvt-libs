using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zvt.Libs.Database
{
    public class TableColumnInfo
    {
        public TableColumnInfo() { }
        public TableColumnInfo(
            string name,
            SqlServerTypes type,
            int length,
            bool isNullable,
            string pkName = null,
            string fkName = null
            )
        {
            this.Name = name;
            this.Type = type;
            this.Length = length;
            this.IsNullable = isNullable;

            if (pkName != null)
            {
                this.IsPk = true;
                this.PkName = pkName;
            }
            else
            {
                this.IsPk = false;
            }

            if (fkName != null)
            {
                this.IsFk = true;
                this.FkName = fkName;
            }
            else
            {
                this.IsFk = false;
            }
        }

        public string Name { get; set; }
        public SqlServerTypes Type { get; set; }
        public int Length { get; set; }
        public bool IsNullable { get; set; }
        public bool IsPk { get; set; }
        public string PkName { get; set; }
        public bool IsFk { get; set; }
        public string FkName { get; set; }
    }
}
