using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Zvt.Libs.Database.Exceptions;

namespace Zvt.Libs.Database
{
    public class SqlServerDatabaseUtils : ISqlServerDatabaseUtils
    {
        public ISqlServerDatabaseHelper DbHelper { get; set; }

        public SqlServerDatabaseUtils(ISqlServerDatabaseHelper dbHelper)
        {
            if (dbHelper == null)
                throw new ArgumentNullException("dbHelper");

            this.DbHelper = dbHelper;
        }

        public List<TableColumnInfo> GetTableColumnsInfo(string tableName)
        {
            var sql = @"

                DECLARE @PKName varchar(max)

                SELECT		@PKName = SO_ConstraintObjs.[name]
                FROM		sysobjects SO_Tbl
                INNER JOIN	sysobjects SO_ConstraintObjs	on SO_ConstraintObjs.[parent_obj] = SO_Tbl.[id]
                WHERE		SO_TBL.[Name] = @TableName

                SELECT		SC.[name] as [Name],
			                ST.[name] as [Type],
			                SC.[length] as [Length],
			                SC.[isnullable] as [Nullable],
			                PKS.[PKName] as [PrimaryKey],
			                FKS.[Name] as [ForeignKey]

                FROM		sysobjects SO
                INNER JOIN	syscolumns SC	on SC.[id] = SO.[id]
                INNER JOIN	systypes ST		on SC.[xtype] = ST.[xtype]

                LEFT JOIN	(	SELECT		SO_ConstraintObjs.[Name],
							                SC.[name] as [RefColumn]
				                FROM		sysobjects SO_Tbl
				                INNER JOIN	sysobjects SO_ConstraintObjs	on SO_ConstraintObjs.[parent_obj] = SO_Tbl.[id]
				                INNER JOIN	sysforeignkeys SFK				on SFK.[constid] = SO_ConstraintObjs.[id]
				                INNER JOIN	sysobjects SO_RefTable			on SO_RefTable.[id] = SFK.[fkeyid]
				                INNER JOIN	syscolumns SC					on SC.[id] = SO_RefTable.[id] and SC.[colid] = SFK.[fkey]
				                WHERE		SO_TBL.[Name] = @TableName
				                AND			SO_ConstraintObjs.[xtype] in ('PK', 'F')
			                )
			                as FKS
			                on FKS.[RefColumn] = SC.[name]

                LEFT JOIN	(	SELECT  sc.[name], 
						                @PKName as [PKName]
				                FROM    syscolumns sc
				                JOIN	sysobjects so ON so.id = sc.id
				                WHERE   so.name = @TableName
				                AND     sc.colid IN (   SELECT		sik.colid
										                FROM		sysindexkeys sik
										                INNER JOIN	sysobjects so		ON sik.[id] = so.[id]
										                WHERE		sik.indid = 1
										                AND			so.name = @TableName
									                )
			                )
			                as PKS
			                on PKS.[name] = SC.[name]

                WHERE		SO.[Name] = @TableName
                AND			ST.[name] <> 'sysname'
            ";

            var sqlParams = new Dictionary<string, object>();
            sqlParams.Add("@TableName", tableName);
            var dt = DbHelper.ExecuteQueryAndReturnTable(sql, sqlParams);
            return MountTableColumnInfoList(dt);
        }
        internal List<TableColumnInfo> MountTableColumnInfoList(DataTable dt)
        {
            if (dt == null)
                throw new ArgumentNullException("dt");

            var list = new List<TableColumnInfo>();

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    var colInfo = new TableColumnInfo();

                    colInfo.Name = row["Name"].ToString();
                    colInfo.Length = Convert.ToInt32(row["Length"]);
                    colInfo.IsNullable = Convert.ToBoolean(row["Nullable"]);

                    try
                    {
                        var type = row["Type"].ToString();
                        colInfo.Type = (SqlServerTypes)Enum.Parse(typeof(SqlServerTypes), type, true);
                    }
                    catch (Exception ex)
                    {
                        throw new SqlServerDatabaseUtilsException("Could not parse table column type value to SqlServerTypes enum.", ex);
                    }

                    if (row.IsNull("PrimaryKey"))
                    {
                        colInfo.IsPk = false;
                    }
                    else
                    {
                        colInfo.IsPk = true;
                        colInfo.PkName = row["PrimaryKey"].ToString();
                    }

                    if (row.IsNull("ForeignKey"))
                    {
                        colInfo.IsFk = false;
                    }
                    else
                    {
                        colInfo.IsFk = true;
                        colInfo.FkName = row["ForeignKey"].ToString();
                    }

                    list.Add(colInfo);
                }
            }

            return list;
        }

        public Type GetDotNetTypeFromSqlType(SqlServerTypes sqlType)
        {
            switch (sqlType)
            {
                case SqlServerTypes.NTEXT:
                case SqlServerTypes.NVARCHAR:
                case SqlServerTypes.TEXT:
                case SqlServerTypes.VARCHAR:
                    return typeof(string);

                case SqlServerTypes.CHAR:
                    return typeof(char);

                case SqlServerTypes.UNIQUEIDENTIFIER:
                    return typeof(Guid);

                case SqlServerTypes.DATE:
                case SqlServerTypes.TIME:
                case SqlServerTypes.DATETIME2:
                case SqlServerTypes.DATETIMEOFFSET:
                case SqlServerTypes.SMALLDATETIME:
                case SqlServerTypes.DATETIME:
                    return typeof(DateTime);
                
                case SqlServerTypes.TINYINT:
                    return typeof(byte);

                case SqlServerTypes.SMALLINT:
                    return typeof(Int16);

                case SqlServerTypes.INT:
                    return typeof(Int32);

                case SqlServerTypes.BIGINT:
                    return typeof(Int64);

                case SqlServerTypes.REAL:
                case SqlServerTypes.MONEY:
                case SqlServerTypes.DECIMAL:
                case SqlServerTypes.NUMERIC:
                case SqlServerTypes.SMALLMONEY:
                    return typeof(decimal);

                case SqlServerTypes.FLOAT:
                    return typeof(float);

                case SqlServerTypes.BIT:
                    return typeof(bool);

                default:
                    throw new SqlServerDatabaseUtilsException("The sql type '" + sqlType.ToString() + "' is not mapped at GetDotNetTypeFromSqlType.");
            }
        }
    }
}
