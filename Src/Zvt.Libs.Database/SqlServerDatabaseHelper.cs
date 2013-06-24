using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Zvt.Libs.Database
{
    public class SqlServerDatabaseHelper : ISqlServerDatabaseHelper
    {
        protected string ConnectionString { get; set; }
        protected SqlConnection TransactionConnection { get; set; }
        protected SqlTransaction Transaction { get; set; }

        public bool IsTransactionActive { get; protected set; }

        public SqlServerDatabaseHelper(
            string connectionString
            )
        {
            if (connectionString == null)
                throw new ArgumentNullException("connectionStringName");

            this.ConnectionString = connectionString;
            this.IsTransactionActive = false;
            this.TransactionConnection = null;
            this.Transaction = null;
        }

        protected SqlConnection GetConnection()
        {
            if (IsTransactionActive)
            {
                return TransactionConnection;
            }
            else
            {
                return new SqlConnection(this.ConnectionString);
            }
        }
        protected SqlCommand GetCommand(
            string sql,
            Dictionary<string, object> sqlParams = null
            )
        {
            if (sql == null)
                throw new ArgumentNullException("sql");

            var objConn = GetConnection();
            var objCmd = new SqlCommand(sql, objConn);

            if (IsTransactionActive)
                objCmd.Transaction = Transaction;

            if (sqlParams != null)
                foreach (var key in sqlParams.Keys)
                    objCmd.Parameters.AddWithValue(key, sqlParams[key]);

            return objCmd;
        }

        public bool VerifyConnection(out string errMsg)
        {
            errMsg = string.Empty;
            var conn = GetConnection();

            try
            {
                conn.Open();
                conn.Close();
                conn.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }
        }

        public void BeginTransaction()
        {
            if (IsTransactionActive) return;

            TransactionConnection = GetConnection();
            TransactionConnection.Open();
            Transaction = TransactionConnection.BeginTransaction();
            IsTransactionActive = true;
        }
        public void CommitTransaction()
        {
            if (!IsTransactionActive) return;

            Transaction.Commit();
            TransactionConnection.Close();
            IsTransactionActive = false;
        }
        public void RollbackTransaction()
        {
            if (!IsTransactionActive) return;

            Transaction.Rollback();
            TransactionConnection.Close();
            IsTransactionActive = false;
        }

        public int ExecuteNonQuery(string sql, Dictionary<string, object> sqlParams = null)
        {
            var objCmd = GetCommand(sql, sqlParams);

            if (!IsTransactionActive)
                objCmd.Connection.Open();

            var affectedRows = objCmd.ExecuteNonQuery();

            if (!IsTransactionActive)
                objCmd.Connection.Close();

            return affectedRows;
        }
        public object ExecuteScalarOrNull(string sql, Dictionary<string, object> sqlParams = null)
        {
            var objCmd = GetCommand(sql, sqlParams);

            if (!IsTransactionActive)
                objCmd.Connection.Open();

            var value = objCmd.ExecuteScalar();

            if (!IsTransactionActive)
                objCmd.Connection.Close();

            return value;
        }
        public TResult ExecuteScalar<TResult>(string sql, Dictionary<string, object> sqlParams = null)
        {
            var objValue = ExecuteScalarOrNull(sql, sqlParams);

            try
            {
                var typedValue = Convert.ChangeType(objValue, typeof(TResult));
                return (TResult)typedValue;
            }
            catch (Exception ex)
            {
                throw new Exception("The query result could not be converted to requested type.", ex);
            }
        }
        public DataTable ExecuteQueryAndReturnTable(string sql, Dictionary<string, object> sqlParams = null)
        {
            var objCmd = GetCommand(sql, sqlParams);
            var objAdapter = new SqlDataAdapter(objCmd);
            var dt = new DataTable();
            objAdapter.Fill(dt);
            objAdapter.Dispose();
            return dt;
        }
        public DataSet ExecuteQuery(string sql, Dictionary<string, object> sqlParams = null)
        {
            var objCmd = GetCommand(sql, sqlParams);
            var objAdapter = new SqlDataAdapter(objCmd);
            var ds = new DataSet();
            objAdapter.Fill(ds);
            objAdapter.Dispose();
            return ds;
        }
        public DataTable ExecuteStoredProcedureAndReturnTable(string spName, Dictionary<string, object> sqlParams = null)
        {
            var objCmd = GetCommand(spName, sqlParams);
            objCmd.CommandType = CommandType.StoredProcedure;
            var objAdapter = new SqlDataAdapter(objCmd);
            var dt = new DataTable();
            objAdapter.Fill(dt);
            objAdapter.Dispose();
            return dt;
        }
        public DataSet ExecuteStoredProcedure(string spName, Dictionary<string, object> sqlParams = null)
        {
            var objCmd = GetCommand(spName, sqlParams);
            objCmd.CommandType = CommandType.StoredProcedure;
            var objAdapter = new SqlDataAdapter(objCmd);
            var ds = new DataSet();
            objAdapter.Fill(ds);
            objAdapter.Dispose();
            return ds;
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
            var dt = ExecuteQueryAndReturnTable(sql, sqlParams);
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
                        throw new SqlServerDatabaseException("Could not parse table column type value to SqlServerTypes enum.", ex);
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
                    throw new SqlServerDatabaseException("The sql type '" + sqlType.ToString() + "' is not mapped at GetDotNetTypeFromSqlType.");
            }
        }
    }
}